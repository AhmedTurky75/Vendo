using MediatR;
using Vendo.Order.Application.Common;
using Vendo.Order.Application.DTOs;

namespace Vendo.Order.Application.Queries.GetOrdersByCustomer;

public class GetOrdersByCustomerQuery : IRequest<Result<List<OrderDto>>>
{
    public Guid CustomerId { get; set; }
}
