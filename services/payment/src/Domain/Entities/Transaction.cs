using Vendo.PaymentManagement.Domain.Enums;

namespace Vendo.PaymentManagement.Domain.Entities;

/// <summary>
/// Represents a transaction associated with a payment.
/// </summary>
public class Transaction
{
    private Transaction() { }

    public Guid Id { get; private set; }
    public Guid PaymentId { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? GatewayTransactionId { get; private set; }
    public string? GatewayResponse { get; private set; }
    public DateTime ProcessedAt { get; private set; }

    /// <summary>
    /// Creates a new transaction.
    /// </summary>
    public static Transaction Create(
        Guid paymentId,
        TransactionType type,
        decimal amount,
        PaymentStatus status,
        string? gatewayTransactionId = null,
        string? gatewayResponse = null)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            PaymentId = paymentId,
            Type = type,
            Amount = amount,
            Status = status,
            GatewayTransactionId = gatewayTransactionId,
            GatewayResponse = gatewayResponse,
            ProcessedAt = DateTime.UtcNow
        };

        return transaction;
    }

    /// <summary>
    /// Updates the transaction status and gateway response.
    /// </summary>
    public void UpdateStatus(PaymentStatus status, string? gatewayResponse = null)
    {
        Status = status;
        if (gatewayResponse != null)
        {
            GatewayResponse = gatewayResponse;
        }
    }
}
