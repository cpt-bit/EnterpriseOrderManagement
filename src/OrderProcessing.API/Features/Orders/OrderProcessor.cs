namespace OrderProcessing.API.Features.Orders;

public class OrderProcessor(IOrderRepository repository, ILogger<OrderProcessor> logger)
{
    public async Task<Order?> ProcessOrderAsync(int id)
    {
        logger.LogInformation("Processing order ID: {OrderId}", id);
        var order = await repository.GetByIdAsync(id);

        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found", id);
        }

        return order;
    }

    public async Task<bool> ApplyDiscountAsync(int id, decimal discountPercent)
    {
        if (discountPercent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(discountPercent), "Discount must be between 0 and 100.");

        var order = await repository.GetByIdAsync(id);
        if (order is null) return false;

        var discountedAmount = order.Amount * (1 - (discountPercent / 100m));
        var updatedOrder = order with { Amount = discountedAmount };

        await repository.SaveAsync(updatedOrder);
        return true;
    }
}