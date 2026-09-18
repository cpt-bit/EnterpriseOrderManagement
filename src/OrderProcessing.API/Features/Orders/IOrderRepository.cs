namespace OrderProcessing.API.Features.Orders;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetAllAsync();
    Task SaveAsync(Order order);
}
