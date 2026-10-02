namespace OrderProcessing.API.Features.Orders;

public class OrderProcessor(IOrderRepository repository, ILogger<OrderProcessor> logger)
{
    public async Task<Order?> ProcessOrderAsync(int id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Processing order ID: {OrderId}", id);
        var order = await repository.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found", id);
        }

        return order;
    }

    public async Task<bool> ApplyDiscountAsync(int id, decimal discountPercent, CancellationToken cancellationToken = default)
    {
        if (discountPercent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(discountPercent), "Discount must be between 0 and 100.");

        var order = await repository.GetByIdAsync(id, cancellationToken);
        if (order is null) return false;

        var discountedAmount = order.Amount * (1 - (discountPercent / 100m));
        var updatedOrder = order with { Amount = discountedAmount };

        await repository.SaveAsync(updatedOrder, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteOrderAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await repository.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found to delete", id);
            return false;
        }

        await repository.DeleteAsync(id, cancellationToken);
        logger.LogInformation("Deleted order ID: {OrderId}", id);
        return true;
    }
}