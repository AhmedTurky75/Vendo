using MediatR;
using Vendo.PaymentManagement.Application.Common.Models;
using Vendo.PaymentManagement.Application.DTOs;

namespace Vendo.PaymentManagement.Application.Queries.GetPayment;

/// <summary>
/// Query to get a payment by ID.
/// </summary>
public sealed class GetPaymentQuery : IRequest<Result<PaymentDto>>
{
    public Guid Id { get; set; }
}
