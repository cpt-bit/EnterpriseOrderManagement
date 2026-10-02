using MassTransit;
using OrderProcessing.API.Data;
using OrderProcessing.API.Middleware;

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
            IPublishEndpoint publishEndpoint,
            CancellationToken cancellationToken) =>
        {
            // 1. Create order entity with Id = 0 (SQLite auto-increments this)
            var order = new Order(0, request.CustomerName, request.Amount);

            // 2. Persist to SQLite database
            await repository.SaveAsync(order, cancellationToken);

            // 3. Publish event to Outbox change tracker
            await publishEndpoint.Publish(new OrderCreatedEvent(
                order.Id,
                order.CustomerName,
                order.Amount,
                DateTime.UtcNow
            ), cancellationToken);

            // 4. Persist the staged outbox message to SQLite
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/orders/{order.Id}", order);
        })
            .AddEndpointFilter<ValidationFilter<CreateOrderRequest>>();


        ordersGroup.MapGet("/{id:int}", async (
            int id,
            OrderProcessor processor,
            CancellationToken cancellationToken) =>
        {
            var order = await processor.ProcessOrderAsync(id, cancellationToken);

            return order is not null
                ? Results.Ok(order)
                : Results.NotFound($"Order {id} not found.");
        });

        ordersGroup.MapPost("/{id:int}/discount", async (
            int id,
            decimal percent,
            OrderProcessor processor,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var success = await processor.ApplyDiscountAsync(id, percent, cancellationToken);

                return success
                    ? Results.NoContent()
                    : Results.NotFound($"Order {id} not found.");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        ordersGroup.MapDelete("/{id:int}", async (
            int id,
            OrderProcessor processor,
            CancellationToken cancellationToken) =>
        {
            var success = await processor.DeleteOrderAsync(id, cancellationToken);

            return success
                ? Results.NoContent() // 204 No Content on successful deletion
                : Results.NotFound($"Order {id} not found.");
        });


        return app;
    }
}