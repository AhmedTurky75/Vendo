using Vendo.Payment.Domain.Enums;

namespace Vendo.Payment.Domain.Entities;

/// <summary>
/// Represents a payment transaction in the system.
/// </summary>
public class Payment
{
    private Payment() { }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime? PaymentDate { get; private set; }
    public string? TransactionId { get; private set; }
    public string? GatewayResponse { get; private set; }
    public decimal? RefundAmount { get; private set; }
    public string? RefundReason { get; private set; }
    public DateTime? RefundDate { get; private set; }
    public string? Metadata { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Creates a new payment.
    /// </summary>
    public static Payment Create(
        Guid tenantId,
        Guid orderId,
        Guid customerId,
        decimal amount,
        string currency,
        PaymentMethod paymentMethod)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required", nameof(currency));
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrderId = orderId,
            CustomerId = customerId,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            PaymentMethod = paymentMethod,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return payment;
    }

    /// <summary>
    /// Processes the payment.
    /// </summary>
    public void Process(string transactionId, string? gatewayResponse = null)
    {
        if (Status != PaymentStatus.Pending && Status != PaymentStatus.Processing)
        {
            throw new InvalidOperationException($"Cannot process payment in {Status} status");
        }

        Status = PaymentStatus.Processing;
        TransactionId = transactionId;
        GatewayResponse = gatewayResponse;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Completes the payment successfully.
    /// </summary>
    public void Complete(string? gatewayResponse = null)
    {
        if (Status != PaymentStatus.Processing)
        {
            throw new InvalidOperationException($"Cannot complete payment in {Status} status");
        }

        Status = PaymentStatus.Completed;
        PaymentDate = DateTime.UtcNow;
        if (gatewayResponse != null)
        {
            GatewayResponse = gatewayResponse;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the payment as failed.
    /// </summary>
    public void Fail(string? gatewayResponse = null)
    {
        if (Status != PaymentStatus.Processing)
        {
            throw new InvalidOperationException($"Cannot fail payment in {Status} status");
        }

        Status = PaymentStatus.Failed;
        if (gatewayResponse != null)
        {
            GatewayResponse = gatewayResponse;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels the payment.
    /// </summary>
    public void Cancel()
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot cancel payment in {Status} status");
        }

        Status = PaymentStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Refunds the payment partially or fully.
    /// </summary>
    public void Refund(decimal refundAmount, string reason)
    {
        if (Status != PaymentStatus.Completed && Status != PaymentStatus.PartiallyRefunded)
        {
            throw new InvalidOperationException($"Cannot refund payment in {Status} status");
        }

        if (refundAmount <= 0)
        {
            throw new ArgumentException("Refund amount must be greater than zero", nameof(refundAmount));
        }

        var currentRefundAmount = RefundAmount ?? 0;
        var totalRefund = currentRefundAmount + refundAmount;

        if (totalRefund > Amount)
        {
            throw new ArgumentException("Total refund amount cannot exceed payment amount", nameof(refundAmount));
        }

        RefundAmount = totalRefund;
        RefundReason = reason;
        RefundDate = DateTime.UtcNow;
        Status = totalRefund >= Amount ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates payment metadata.
    /// </summary>
    public void UpdateMetadata(string metadata)
    {
        Metadata = metadata;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates basic payment information.
    /// </summary>
    public void Update(decimal? amount = null, string? currency = null, PaymentMethod? paymentMethod = null)
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot update payment in {Status} status");
        }

        if (amount.HasValue)
        {
            if (amount.Value <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero", nameof(amount));
            }
            Amount = amount.Value;
        }

        if (!string.IsNullOrWhiteSpace(currency))
        {
            Currency = currency.ToUpperInvariant();
        }

        if (paymentMethod.HasValue)
        {
            PaymentMethod = paymentMethod.Value;
        }

        UpdatedAt = DateTime.UtcNow;
    }
}
