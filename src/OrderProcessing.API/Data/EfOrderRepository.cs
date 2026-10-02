using Microsoft.EntityFrameworkCore;
using OrderProcessing.API.Features.Orders;

namespace OrderProcessing.API.Data;

public class EfOrderRepository(AppDbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // AsNoTracking avoids change-tracking overhead for read-only queries
        return await context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Orders.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(Order order, CancellationToken cancellationToken = default)
    {
        if (order.Id == 0)
        {
            await context.Orders.AddAsync(order, cancellationToken);
        }
        else
        {
            context.Orders.Update(order);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await context.Orders.FindAsync([id], cancellationToken);
        if (order is not null)
        {
            context.Orders.Remove(order);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}