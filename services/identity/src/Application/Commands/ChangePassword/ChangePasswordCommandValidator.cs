using FluentValidation;

namespace Vendo.Identity.Application.Commands.ChangePassword;

/// <summary>
/// Validator for ChangePasswordCommand
/// </summary>
public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from current password");
    }
}
