using MediatR;
using Vendo.Order.Application.Common;
using Vendo.Order.Application.DTOs;

namespace Vendo.Order.Application.Queries.GetOrder;

public class GetOrderQuery : IRequest<Result<OrderDto>>
{
    public Guid Id { get; set; }
}
