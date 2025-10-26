using FluentValidation;

namespace Vendo.CatalogManagement.Application.Categories.Commands.DeleteCategory;

/// <summary>
/// Validator for DeleteCategoryCommand.
/// </summary>
public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Category ID is required");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required");
    }
}
