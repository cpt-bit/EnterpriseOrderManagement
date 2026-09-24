using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OrderProcessing.API.Data;
using OrderProcessing.API.Extensions;
using OrderProcessing.API.Features.Orders;

var builder = WebApplication.CreateBuilder(args);

// Register application & infrastructure services
builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);
builder.Services.AddMessagingServices(builder.Configuration);
builder.Services.AddHealthCheckServices(builder.Configuration);

var app = builder.Build();

// 2. Safe Local Database Initialization
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map health check endpoints
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = reg => reg.Tags.Contains("ready") });

app.MapOrderEndpoints();

app.Run();

public partial class Program { }