using FluentValidation;

namespace Vendo.PaymentManagement.Application.Commands.DeletePayment;

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
