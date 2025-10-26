using MediatR;
using Vendo.OrderManagement.Application.Common;
using Vendo.OrderManagement.Application.DTOs;

namespace Vendo.OrderManagement.Application.Commands.CancelOrder;

public class CancelOrderCommand : IRequest<Result<OrderDto>>
{
    public Guid Id { get; set; }
}
