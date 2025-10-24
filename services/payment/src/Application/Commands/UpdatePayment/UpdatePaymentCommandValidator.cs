using FluentValidation;

namespace Vendo.Payment.Application.Commands.UpdatePayment;

/// <summary>
/// Validator for UpdatePaymentCommand.
/// </summary>
public class UpdatePaymentCommandValidator : AbstractValidator<UpdatePaymentCommand>
{
    public UpdatePaymentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Payment ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero")
            .When(x => x.Amount.HasValue);

        RuleFor(x => x.Currency)
            .Length(3).WithMessage("Currency must be 3 characters (ISO 4217 code)")
            .Matches(@"^[A-Z]{3}$").WithMessage("Currency must be uppercase ISO 4217 code")
            .When(x => !string.IsNullOrWhiteSpace(x.Currency));

        RuleFor(x => x.PaymentMethod)
            .Must(BeValidPaymentMethod).WithMessage("Invalid payment method")
            .When(x => !string.IsNullOrWhiteSpace(x.PaymentMethod));
    }

    private bool BeValidPaymentMethod(string? paymentMethod)
    {
        if (string.IsNullOrWhiteSpace(paymentMethod)) return true;
        var validMethods = new[] { "CreditCard", "DebitCard", "PayPal", "Stripe", "BankTransfer", "Cash", "Other" };
        return validMethods.Contains(paymentMethod);
    }
}
