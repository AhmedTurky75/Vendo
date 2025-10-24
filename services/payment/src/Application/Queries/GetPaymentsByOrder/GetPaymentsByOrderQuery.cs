using MediatR;
using Vendo.Payment.Application.Common.Models;
using Vendo.Payment.Application.DTOs;

namespace Vendo.Payment.Application.Queries.GetPaymentsByOrder;

/// <summary>
/// Query to get all payments for an order.
/// </summary>
public sealed class GetPaymentsByOrderQuery : IRequest<Result<List<PaymentDto>>>
{
    public Guid OrderId { get; set; }
}
