using MediatR;
using Vendo.OrderManagement.Application.Common;
using Vendo.OrderManagement.Application.DTOs;

namespace Vendo.OrderManagement.Application.Queries.GetOrders;

public class GetOrdersQuery : IRequest<Result<List<OrderDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
