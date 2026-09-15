namespace OrderProcessing.API;

public record Order(int Id, string CustomerName, decimal Amount);

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetAllAsync();
    Task SaveAsync(Order order);
}

public class InMemoryOrderRepository() : IOrderRepository
{
    private readonly List<Order> _orders = 
    [
        new(1, "Acme Corp", 1500m),
        new(2, "Apex Ltd", 3200m)
    ];

    public Task<Order?> GetByIdAsync(int id) => 
        Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));

    public Task<IEnumerable<Order>> GetAllAsync() => 
        Task.FromResult(_orders.AsEnumerable());
    
    public Task SaveAsync(Order order)
    {
        var existingIndex = _orders.FindIndex(o => o.Id == order.Id);
        if (existingIndex >= 0)
        {
            _orders[existingIndex] = order;
        }
        else
        {
            _orders.Add(order);
        }

        return Task.CompletedTask;
    }
}

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