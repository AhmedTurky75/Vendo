using MediatR;
using Vendo.Payment.Application.Common.Models;
using Vendo.Payment.Application.DTOs;

namespace Vendo.Payment.Application.Commands.RefundPayment;

/// <summary>
/// Command to refund a payment.
/// </summary>
public sealed class RefundPaymentCommand : IRequest<Result<PaymentDto>>
{
    public Guid Id { get; set; }
    public decimal RefundAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
}
