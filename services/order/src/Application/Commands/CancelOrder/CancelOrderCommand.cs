using MediatR;
using Vendo.Order.Application.Common;
using Vendo.Order.Application.DTOs;

namespace Vendo.Order.Application.Commands.CancelOrder;

public class CancelOrderCommand : IRequest<Result<OrderDto>>
{
    public Guid Id { get; set; }
}
