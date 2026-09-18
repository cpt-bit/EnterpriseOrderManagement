namespace OrderProcessing.API.Features.Orders;

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
