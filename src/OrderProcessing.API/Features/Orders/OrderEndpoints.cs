namespace OrderProcessing.API.Features.Orders;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var ordersGroup = app.MapGroup("/api/orders");

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
