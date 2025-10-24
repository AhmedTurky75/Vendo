using MediatR;
using Vendo.Payment.Application.Common.Models;
using Vendo.Payment.Application.DTOs;

namespace Vendo.Payment.Application.Queries.GetPayment;

/// <summary>
/// Query to get a payment by ID.
/// </summary>
public sealed class GetPaymentQuery : IRequest<Result<PaymentDto>>
{
    public Guid Id { get; set; }
}
