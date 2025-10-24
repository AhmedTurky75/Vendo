using MediatR;
using Vendo.Payment.Application.Common.Models;
using Vendo.Payment.Application.DTOs;

namespace Vendo.Payment.Application.Commands.UpdatePayment;

/// <summary>
/// Command to update a payment.
/// </summary>
public sealed class UpdatePaymentCommand : IRequest<Result<PaymentDto>>
{
    public Guid Id { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Metadata { get; set; }
}
