using MediatR;
using Vendo.OrderManagement.Application.Common;
using Vendo.OrderManagement.Application.DTOs;
using Vendo.OrderManagement.Domain.Enums;

namespace Vendo.OrderManagement.Application.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommand : IRequest<Result<OrderDto>>
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
}
