using MediatR;
using Vendo.PaymentManagement.Application.Common.Models;
using Vendo.PaymentManagement.Application.DTOs;

namespace Vendo.PaymentManagement.Application.Commands.CreatePayment;

/// <summary>
/// Command to create a new payment.
/// </summary>
public sealed class CreatePaymentCommand : IRequest<Result<PaymentDto>>
{
    public Guid TenantId { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Metadata { get; set; }
}
