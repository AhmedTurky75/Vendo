namespace Vendo.Payment.Domain.Enums;

/// <summary>
/// Transaction operation types.
/// </summary>
public enum TransactionType
{
    Charge,
    Refund,
    Void,
    Chargeback
}
