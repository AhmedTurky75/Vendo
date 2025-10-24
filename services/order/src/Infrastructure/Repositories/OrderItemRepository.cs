using Microsoft.EntityFrameworkCore;
using Vendo.Order.Domain.Entities;
using Vendo.Order.Domain.Repositories;
using Vendo.Order.Infrastructure.Persistence;

namespace Vendo.Order.Infrastructure.Repositories;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly OrderDbContext _context;

    public OrderItemRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderItem>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderItems
            .Where(oi => oi.OrderId == orderId)
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderItem> AddAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
    {
        await _context.OrderItems.AddAsync(orderItem, cancellationToken);
        return orderItem;
    }

    public Task UpdateAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
    {
        _context.OrderItems.Update(orderItem);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
    {
        _context.OrderItems.Remove(orderItem);
        return Task.CompletedTask;
    }
}
