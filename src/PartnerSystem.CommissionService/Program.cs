using Microsoft.EntityFrameworkCore;
using PartnerSystem.CommissionService.Data;
using PartnerSystem.CommissionService.Extensions;
using PartnerSystem.CommissionService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CommissionDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services
    .RegisterAppServices()
    .AddUpstreamClients(builder.Configuration);

builder.Services.AddHostedService<KafkaConsumer>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CommissionDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
