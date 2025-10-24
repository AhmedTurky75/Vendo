using FluentValidation;

namespace Vendo.Payment.Application.Commands.CreatePayment;

/// <summary>
/// Validator for CreatePaymentCommand.
/// </summary>
public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId is required");

        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be 3 characters (ISO 4217 code)")
            .Matches(@"^[A-Z]{3}$").WithMessage("Currency must be uppercase ISO 4217 code");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("PaymentMethod is required")
            .Must(BeValidPaymentMethod).WithMessage("Invalid payment method");
    }

    private bool BeValidPaymentMethod(string paymentMethod)
    {
        var validMethods = new[] { "CreditCard", "DebitCard", "PayPal", "Stripe", "BankTransfer", "Cash", "Other" };
        return validMethods.Contains(paymentMethod);
    }
}
