namespace Vendo.PaymentManagement.Application.DTOs;

/// <summary>
/// Data transfer object for Transaction.
/// </summary>
public class TransactionDto
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? GatewayTransactionId { get; set; }
    public string? GatewayResponse { get; set; }
    public DateTime ProcessedAt { get; set; }
}
