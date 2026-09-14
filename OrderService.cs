namespace OrderProcessing.API;

// C# 12 Record with Primary Constructor
public record Order(int Id, string CustomerName, decimal Amount);

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetAllAsync();
}

// C# 12 Primary Constructor + Collection Expression []
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
}

// C# 12 Primary Constructor for Dependency Injection
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
}