using MediatR;
using Vendo.OrderManagement.Application.Common;
using Vendo.OrderManagement.Application.DTOs;

namespace Vendo.OrderManagement.Application.Queries.GetOrder;

public class GetOrderQuery : IRequest<Result<OrderDto>>
{
    public Guid Id { get; set; }
}
