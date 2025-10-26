using FluentValidation;

namespace Vendo.IdentityManagement.Application.Commands.ForgotPassword;

/// <summary>
/// Validator for ForgotPasswordCommand
/// </summary>
public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}
