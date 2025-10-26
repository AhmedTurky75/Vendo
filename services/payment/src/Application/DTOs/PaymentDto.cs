namespace Vendo.PaymentManagement.Application.DTOs;

/// <summary>
/// Data transfer object for Payment.
/// </summary>
public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? PaymentDate { get; set; }
    public string? TransactionId { get; set; }
    public string? GatewayResponse { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? RefundReason { get; set; }
    public DateTime? RefundDate { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
