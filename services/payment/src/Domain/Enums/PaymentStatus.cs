namespace Vendo.PaymentManagement.Domain.Enums;

/// <summary>
/// Payment status lifecycle states.
/// </summary>
public enum PaymentStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled,
    Refunded,
    PartiallyRefunded
}
