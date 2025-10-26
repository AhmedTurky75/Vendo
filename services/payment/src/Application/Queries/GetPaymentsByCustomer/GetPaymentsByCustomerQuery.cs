using MediatR;
using Vendo.PaymentManagement.Application.Common.Models;
using Vendo.PaymentManagement.Application.DTOs;

namespace Vendo.PaymentManagement.Application.Queries.GetPaymentsByCustomer;

/// <summary>
/// Query to get all payments for a customer.
/// </summary>
public sealed class GetPaymentsByCustomerQuery : IRequest<Result<List<PaymentDto>>>
{
    public Guid CustomerId { get; set; }
}
