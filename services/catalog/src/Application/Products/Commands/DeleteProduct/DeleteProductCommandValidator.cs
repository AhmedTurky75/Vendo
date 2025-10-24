using FluentValidation;

namespace Vendo.Catalog.Application.Products.Commands.DeleteProduct;

/// <summary>
/// Validator for DeleteProductCommand.
/// </summary>
public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required");
    }
}
