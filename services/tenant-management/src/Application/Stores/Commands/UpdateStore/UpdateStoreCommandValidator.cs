using FluentValidation;

namespace Vendo.TenantManagement.Application.Stores.Commands.UpdateStore;

/// <summary>
/// Validator for UpdateStoreCommand.
/// </summary>
public class UpdateStoreCommandValidator : AbstractValidator<UpdateStoreCommand>
{
    public UpdateStoreCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Store ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Store name is required")
            .MaximumLength(100).WithMessage("Store name cannot exceed 100 characters")
            .MinimumLength(2).WithMessage("Store name must be at least 2 characters");

        RuleFor(x => x.MerchantEmail)
            .NotEmpty().WithMessage("Merchant email is required")
            .EmailAddress().WithMessage("Invalid email address format");

        RuleFor(x => x.MerchantPhone)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.MerchantPhone));

        RuleFor(x => x.MerchantBusinessName)
            .MaximumLength(200).WithMessage("Business name cannot exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.MerchantBusinessName));

        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0, 100).WithMessage("Tax rate must be between 0 and 100")
            .When(x => x.TaxRate.HasValue);

        RuleFor(x => x.PrimaryColor)
            .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
            .WithMessage("Primary color must be a valid hex color code (e.g., #FF5733)")
            .When(x => !string.IsNullOrWhiteSpace(x.PrimaryColor));

        RuleFor(x => x.AccentColor)
            .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
            .WithMessage("Accent color must be a valid hex color code (e.g., #FF5733)")
            .When(x => !string.IsNullOrWhiteSpace(x.AccentColor));
    }
}
