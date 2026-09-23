using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderProcessing.API.Data;
using OrderProcessing.API.Features.Orders;

namespace OrderProcessing.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Resolve SQLite path relative to ContentRootPath (works across CLI, IDE, Docker, and Publish)
        if (!string.IsNullOrEmpty(connectionString) && connectionString.Contains("Data Source="))
        {
            var relativePath = connectionString.Replace("Data Source=", "").Trim();
            var absolutePath = Path.GetFullPath(Path.Combine(environment.ContentRootPath, relativePath));

            // Ensure target folder exists on disk before EF Core initializes
            var directoryPath = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            connectionString = $"Data Source={absolutePath}";
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IOrderRepository, EfOrderRepository>();
        services.AddScoped<OrderProcessor>();

        return services;
    }

    public static IServiceCollection AddMessagingServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Register MassTransit consumers
            x.AddConsumer<OrderCreatedConsumer>();

            // Enable EF Core Transactional Outbox
            x.AddEntityFrameworkOutbox<AppDbContext>(o =>
            {
                o.UseSqlite();
                o.UseBusOutbox();
            });
            
            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMQ:Host"] ?? "localhost";
                var username = configuration["RabbitMQ:Username"] ?? "guest";
                var password = configuration["RabbitMQ:Password"] ?? "guest";

                cfg.Host(host, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                // Autoconfigure endpoints, exchanges, and queue bindings for registered consumers
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}