using FluentValidation;

namespace Vendo.Payment.Application.Commands.DeletePayment;

/// <summary>
/// Validator for DeletePaymentCommand.
/// </summary>
public class DeletePaymentCommandValidator : AbstractValidator<DeletePaymentCommand>
{
    public DeletePaymentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Payment ID is required");
    }
}
