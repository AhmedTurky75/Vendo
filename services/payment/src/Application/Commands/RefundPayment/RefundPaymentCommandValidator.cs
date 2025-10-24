using FluentValidation;

namespace Vendo.Payment.Application.Commands.RefundPayment;

/// <summary>
/// Validator for RefundPaymentCommand.
/// </summary>
public class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Payment ID is required");

        RuleFor(x => x.RefundAmount)
            .GreaterThan(0).WithMessage("Refund amount must be greater than zero");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Refund reason is required")
            .MaximumLength(500).WithMessage("Refund reason cannot exceed 500 characters");
    }
}
