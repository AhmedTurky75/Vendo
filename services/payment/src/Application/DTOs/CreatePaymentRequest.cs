namespace Vendo.Payment.Application.DTOs;

/// <summary>
/// Request to create a new payment.
/// </summary>
public class CreatePaymentRequest
{
    public Guid TenantId { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Metadata { get; set; }
}
