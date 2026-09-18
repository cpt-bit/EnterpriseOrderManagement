using Microsoft.EntityFrameworkCore;
using OrderProcessing.API.Features.Orders;

namespace OrderProcessing.API.Data;

public class EfOrderRepository(AppDbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(int id)
    {
        // AsNoTracking avoids change-tracking overhead for read-only queries
        return await context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await context.Orders.AsNoTracking().ToListAsync();
    }
    
    public async Task SaveAsync(Order order)
    {
        context.Orders.Update(order);
        await context.SaveChangesAsync();
    }
}