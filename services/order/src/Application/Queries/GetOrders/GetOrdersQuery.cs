using MediatR;
using Vendo.Order.Application.Common;
using Vendo.Order.Application.DTOs;

namespace Vendo.Order.Application.Queries.GetOrders;

public class GetOrdersQuery : IRequest<Result<List<OrderDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
