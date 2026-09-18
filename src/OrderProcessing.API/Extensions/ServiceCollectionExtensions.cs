using Microsoft.EntityFrameworkCore;
using OrderProcessing.API.Data;
using OrderProcessing.API.Features.Orders;

namespace OrderProcessing.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? "Data Source=orders.db";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IOrderRepository, EfOrderRepository>();
        services.AddScoped<OrderProcessor>();

        return services;
    }
}
