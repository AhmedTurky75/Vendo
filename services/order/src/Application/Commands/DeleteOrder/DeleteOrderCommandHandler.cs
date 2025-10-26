using MediatR;
using Vendo.OrderManagement.Application.Common;
using Vendo.OrderManagement.Domain.Repositories;

namespace Vendo.OrderManagement.Application.Commands.DeleteOrder;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, Result<bool>>
{
    private readonly IOrderRepository _orderRepository;

    public DeleteOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<bool>> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order == null)
        {
            return Result<bool>.Failure("Order not found");
        }

        await _orderRepository.DeleteAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true, "Order deleted successfully");
    }
}
