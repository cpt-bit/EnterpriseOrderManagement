using MassTransit;
using OrderProcessing.API.Data;

namespace OrderProcessing.API.Features.Orders;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var ordersGroup = app.MapGroup("/api/orders");

        ordersGroup.MapPost("/", async (
            CreateOrderRequest request,
            IOrderRepository repository,
            AppDbContext dbContext,
            IPublishEndpoint publishEndpoint) =>
        {
            // 1. Create order entity with Id = 0 (SQLite auto-increments this)
            var order = new Order(0, request.CustomerName, request.Amount);

            // 2. Persist to SQLite database
            await repository.SaveAsync(order);

            // 3. Publish event to Outbox change tracker
            await publishEndpoint.Publish(new OrderCreatedEvent(
                order.Id,
                order.CustomerName,
                order.Amount,
                DateTime.UtcNow
            ));

            // 4. Persist the staged outbox message to SQLite
            await dbContext.SaveChangesAsync();

            return Results.Created($"/api/orders/{order.Id}", order);
        });
        
        ordersGroup.MapGet("/{id:int}", async (int id, OrderProcessor processor) =>
        {
            var order = await processor.ProcessOrderAsync(id);

            return order is not null
                ? Results.Ok(order)
                : Results.NotFound($"Order {id} not found.");
        });

        ordersGroup.MapPost("/{id:int}/discount", async (
            int id,
            decimal percent,
            OrderProcessor processor) =>
        {
            try
            {
                var success = await processor.ApplyDiscountAsync(id, percent);

                return success
                    ? Results.NoContent()
                    : Results.NotFound($"Order {id} not found.");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        return app;
    }
}