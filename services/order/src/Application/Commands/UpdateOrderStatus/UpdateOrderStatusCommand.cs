using MediatR;
using Vendo.Order.Application.Common;
using Vendo.Order.Application.DTOs;
using Vendo.Order.Domain.Enums;

namespace Vendo.Order.Application.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommand : IRequest<Result<OrderDto>>
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
}
