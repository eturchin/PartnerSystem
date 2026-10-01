using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.EntityFrameworkCore;
using PartnerSystem.CommissionService.Data;
using PartnerSystem.CommissionService.Data.Entities;
using PartnerSystem.CommissionService.Services.Interfaces;
using PartnerSystem.Contracts.Http;
using PartnerSystem.Shared.Contracts.Enums;
using PartnerSystem.Shared.Contracts.Events;
using PartnerSystem.Shared.Kafka;
using PartnerSystem.Shared.Options;

namespace PartnerSystem.CommissionService.Services;

/// <summary>
/// Consumes <see cref="ProfitEventMessage"/> from Kafka and accrues partner commissions.
/// Offsets are committed only after the event is fully processed (at-least-once).
/// Idempotency is guaranteed by the unique index on (EventId, BeneficiaryUserId).
/// </summary>
internal sealed class KafkaConsumer : BackgroundService
{
    private const string TopicName = KafkaTopics.ProfitEvents;
    private const int TopicPartitions = 1;
    private const short TopicReplicationFactor = 1;
    
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<KafkaConsumer> _logger;

    public KafkaConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration config,
        ILogger<KafkaConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var bootstrapServers = _config["Kafka:BootstrapServers"] ?? "kafka:9092";

        await EnsureTopicExistsWithRetryAsync(bootstrapServers, cancellationToken);

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "commission-service",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        consumer.Subscribe(TopicName);

        _logger.LogInformation("Kafka consumer subscribed to topic {Topic}", TopicName);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var cr = consumer.Consume(cancellationToken);
                if (cr?.Message?.Value is null)
                {
                    continue;
                }

                var message = JsonSerializer.Deserialize<ProfitEventMessage>(cr.Message.Value, JsonOptions.SerializerOptions);
                
                if (message is null)
                {
                    _logger.LogWarning("Skipping malformed message at offset {Offset}", cr.Offset);
                    continue;
                }

                await ProcessEventAsync(message, cancellationToken);

                // Commit only after successful processing; on failure the offset is not committed
                // and the message will be redelivered.
                consumer.Commit(cr);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process profit event");
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        consumer.Close();
    }

    private async Task ProcessEventAsync(ProfitEventMessage message, CancellationToken cancellationToken)
    {
        // Negative profit does not generate commissions.
        if (message.Profit <= 0)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<CommissionDbContext>();
        var usersApi = scope.ServiceProvider.GetRequiredService<IUsersApi>();
        var schemaService = scope.ServiceProvider.GetRequiredService<ICommissionSchemaService>();

        // Idempotency: if commissions for this event already exist, skip.
        var alreadyProcessed = await db.Commissions
            .AnyAsync(c => c.EventId == message.EventId, cancellationToken);

        if (alreadyProcessed)
        {
            _logger.LogDebug("Event {EventId} already processed, skipping", message.EventId);
            return;
        }

        var chain = await usersApi.GetChainUpAsync(message.UserExternalId, cancellationToken);
        var schema = await schemaService.GetCurrentAsync(cancellationToken);

        var commissions = new List<CommissionEntity>(chain.Count);

        for (var i = 0; i < chain.Count; i++)
        {
            var level = i + 1;
            var amount = CommissionCalculator.Calculate(message.Profit, level, schema);
            if (amount <= 0) continue;

            commissions.Add(new CommissionEntity
            {
                EventId = message.EventId,
                BeneficiaryUserId = chain[i],
                SourceUserId = message.UserExternalId,
                Amount = amount,
                Level = level,
                SchemaType = schema,
                Status = CommissionStatus.Accrued,
                AccruedAt = DateTime.UtcNow
            });
        }

        if (commissions.Count == 0)
        {
            _logger.LogDebug("No commissions accrued for event {EventId}", message.EventId);
            return;
        }

        db.Commissions.AddRange(commissions);
        await db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Accrued {Count} commissions for event {EventId}",
            commissions.Count, message.EventId);
    }
    
    private async Task EnsureTopicExistsWithRetryAsync(string bootstrapServers, CancellationToken cancellationToken)
    {
        const int maxAttempts = 10;
        var delay = TimeSpan.FromSeconds(2);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await EnsureTopicExistsAsync(bootstrapServers);
                return;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Attempt {Attempt}/{Max} to ensure Kafka topic failed, retrying in {Delay}s",
                    attempt, maxAttempts, delay.TotalSeconds);

                if (attempt == maxAttempts)
                {
                    _logger.LogError(ex, "Giving up on ensuring Kafka topic after {Max} attempts", maxAttempts);
                    throw;
                }

                await Task.Delay(delay, cancellationToken);
                
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 30));
            }
        }
    }
    
    private async Task EnsureTopicExistsAsync(string bootstrapServers)
    {
        using var admin = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = bootstrapServers
        }).Build();

        try
        {
            var metadata = admin.GetMetadata(TopicName, TimeSpan.FromSeconds(5));
            var existing = metadata.Topics
                .FirstOrDefault(t => t.Topic == TopicName);

            if (existing is not null &&
                existing.Error.Code != ErrorCode.UnknownTopicOrPart)
            {
                _logger.LogDebug("Kafka topic {Topic} already exists", TopicName);
                return;
            }
        }
        catch (KafkaException ex)
        {
            _logger.LogWarning(ex, "Failed to query Kafka metadata, will attempt to create the topic");
        }

        try
        {
            await admin.CreateTopicsAsync([
                new TopicSpecification
                {
                    Name = TopicName,
                    NumPartitions = TopicPartitions,
                    ReplicationFactor = TopicReplicationFactor
                }
            ]);

            _logger.LogInformation(
                "Created Kafka topic {Topic} (partitions={Partitions}, replication={Replication})",
                TopicName, TopicPartitions, TopicReplicationFactor);
        }
        catch (CreateTopicsException ex)
        {
            var topicResult = ex.Results.FirstOrDefault(r => r.Topic == TopicName);

            if (topicResult?.Error.Code == ErrorCode.TopicAlreadyExists)
            {
                _logger.LogDebug("Kafka topic {Topic} was created concurrently", TopicName);
                return;
            }

            _logger.LogError(ex,
                "Failed to create Kafka topic {Topic}: {Reason}",
                TopicName, topicResult?.Error.Reason ?? ex.Message);

            throw;
        }
    }
}