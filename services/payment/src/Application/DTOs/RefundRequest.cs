namespace Vendo.PaymentManagement.Application.DTOs;

/// <summary>
/// Request to refund a payment.
/// </summary>
public class RefundRequest
{
    public decimal RefundAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
}
