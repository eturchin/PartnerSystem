using Microsoft.EntityFrameworkCore;
using PartnerSystem.EventService.Data;
using PartnerSystem.EventService.Extensions;
using PartnerSystem.EventService.Services;

var builder = WebApplication.CreateBuilder(args);

// Persistence
builder.Services.AddDbContext<EventDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Application services
builder.Services.RegisterAppServices();

// Messaging / background
builder.Services.AddSingleton<KafkaProducer>();
builder.Services.AddHostedService<OutboxPublisher>();

// Web
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Apply schema on startup. Replace with EF Core migrations in production.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();