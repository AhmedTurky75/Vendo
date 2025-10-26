using MediatR;
using Vendo.OrderManagement.Application.Common;
using Vendo.OrderManagement.Application.DTOs;

namespace Vendo.OrderManagement.Application.Queries.GetOrdersByCustomer;

public class GetOrdersByCustomerQuery : IRequest<Result<List<OrderDto>>>
{
    public Guid CustomerId { get; set; }
}
