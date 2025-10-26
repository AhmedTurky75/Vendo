using MediatR;
using Vendo.OrderManagement.Application.Common;
using Vendo.OrderManagement.Application.DTOs;
using Vendo.OrderManagement.Domain.Enums;

namespace Vendo.OrderManagement.Application.Queries.GetOrdersByStatus;

public class GetOrdersByStatusQuery : IRequest<Result<List<OrderDto>>>
{
    public OrderStatus Status { get; set; }
}
