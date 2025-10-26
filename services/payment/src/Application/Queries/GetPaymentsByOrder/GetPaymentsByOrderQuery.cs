using MediatR;
using Vendo.PaymentManagement.Application.Common.Models;
using Vendo.PaymentManagement.Application.DTOs;

namespace Vendo.PaymentManagement.Application.Queries.GetPaymentsByOrder;

/// <summary>
/// Query to get all payments for an order.
/// </summary>
public sealed class GetPaymentsByOrderQuery : IRequest<Result<List<PaymentDto>>>
{
    public Guid OrderId { get; set; }
}
