using FluentValidation;

namespace Vendo.Payment.Application.Commands.ProcessPayment;

/// <summary>
/// Validator for ProcessPaymentCommand.
/// </summary>
public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Payment ID is required");

        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("Transaction ID is required");
    }
}
