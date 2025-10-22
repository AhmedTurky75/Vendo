using FluentValidation;

namespace Vendo.TenantManagement.Application.Stores.Commands.CreateStore;

/// <summary>
/// Validator for CreateStoreCommand.
/// </summary>
public class CreateStoreCommandValidator : AbstractValidator<CreateStoreCommand>
{
    public CreateStoreCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Store name is required")
            .MaximumLength(100).WithMessage("Store name cannot exceed 100 characters")
            .MinimumLength(2).WithMessage("Store name must be at least 2 characters");

        RuleFor(x => x.Subdomain)
            .NotEmpty().WithMessage("Subdomain is required")
            .MinimumLength(3).WithMessage("Subdomain must be at least 3 characters")
            .MaximumLength(63).WithMessage("Subdomain cannot exceed 63 characters")
            .Matches(@"^[a-z0-9](?:[a-z0-9-]{1,61}[a-z0-9])?$")
            .WithMessage("Subdomain can only contain lowercase letters, numbers, and hyphens. It must start and end with a letter or number.");

        RuleFor(x => x.MerchantEmail)
            .NotEmpty().WithMessage("Merchant email is required")
            .EmailAddress().WithMessage("Invalid email address format");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required");

        RuleFor(x => x.MerchantPhone)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.MerchantPhone));

        RuleFor(x => x.MerchantBusinessName)
            .MaximumLength(200).WithMessage("Business name cannot exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.MerchantBusinessName));
    }
}
