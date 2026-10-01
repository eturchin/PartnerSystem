using PartnerSystem.AdminService.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddUpstreamClients(builder.Configuration)
    .RegisterAppServices();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
