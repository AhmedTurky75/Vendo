using MediatR;
using Vendo.Payment.Application.Common.Models;
using Vendo.Payment.Application.DTOs;

namespace Vendo.Payment.Application.Queries.GetPaymentsByCustomer;

/// <summary>
/// Query to get all payments for a customer.
/// </summary>
public sealed class GetPaymentsByCustomerQuery : IRequest<Result<List<PaymentDto>>>
{
    public Guid CustomerId { get; set; }
}
