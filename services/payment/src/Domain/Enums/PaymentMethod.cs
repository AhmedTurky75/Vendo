namespace Vendo.Payment.Domain.Enums;

/// <summary>
/// Payment method types supported by the system.
/// </summary>
public enum PaymentMethod
{
    CreditCard,
    DebitCard,
    PayPal,
    Stripe,
    BankTransfer,
    Cash,
    Other
}
