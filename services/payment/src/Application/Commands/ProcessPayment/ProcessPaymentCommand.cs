using MediatR;
using Vendo.Payment.Application.Common.Models;
using Vendo.Payment.Application.DTOs;

namespace Vendo.Payment.Application.Commands.ProcessPayment;

/// <summary>
/// Command to process a payment.
/// </summary>
public sealed class ProcessPaymentCommand : IRequest<Result<PaymentDto>>
{
    public Guid Id { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string? GatewayResponse { get; set; }
    public bool IsSuccess { get; set; }
}
