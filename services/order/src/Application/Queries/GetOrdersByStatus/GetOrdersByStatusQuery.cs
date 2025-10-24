using MediatR;
using Vendo.Order.Application.Common;
using Vendo.Order.Application.DTOs;
using Vendo.Order.Domain.Enums;

namespace Vendo.Order.Application.Queries.GetOrdersByStatus;

public class GetOrdersByStatusQuery : IRequest<Result<List<OrderDto>>>
{
    public OrderStatus Status { get; set; }
}
