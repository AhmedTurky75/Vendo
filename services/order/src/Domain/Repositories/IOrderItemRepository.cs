using Vendo.Order.Domain.Entities;

namespace Vendo.Order.Domain.Repositories;

public interface IOrderItemRepository
{
    Task<IEnumerable<OrderItem>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<OrderItem> AddAsync(OrderItem orderItem, CancellationToken cancellationToken = default);
    Task UpdateAsync(OrderItem orderItem, CancellationToken cancellationToken = default);
    Task DeleteAsync(OrderItem orderItem, CancellationToken cancellationToken = default);
}
