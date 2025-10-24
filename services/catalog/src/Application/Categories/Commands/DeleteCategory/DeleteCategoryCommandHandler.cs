using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.Catalog.Application.Common;
using Vendo.Catalog.Domain.Interfaces;

namespace Vendo.Catalog.Application.Categories.Commands.DeleteCategory;

/// <summary>
/// Handler for deleting a category.
/// </summary>
public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result<bool>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<DeleteCategoryCommandHandler> _logger;

    public DeleteCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        ILogger<DeleteCategoryCommandHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting category with ID: {CategoryId} for tenant: {TenantId}", request.Id, request.TenantId);

        // Get existing category with products
        var category = await _categoryRepository.GetWithProductsAsync(request.Id, cancellationToken);
        if (category == null)
        {
            return Result<bool>.Failure("Category not found");
        }

        // Verify tenant ownership
        if (category.TenantId != request.TenantId)
        {
            return Result<bool>.Failure("Category does not belong to this tenant");
        }

        // Check if category has products
        if (category.Products?.Any() == true)
        {
            return Result<bool>.Failure("Cannot delete category with existing products. Please reassign or delete products first.");
        }

        // Check if category has child categories
        var childCategories = await _categoryRepository.GetChildCategoriesAsync(request.Id, cancellationToken);
        if (childCategories.Any())
        {
            return Result<bool>.Failure("Cannot delete category with child categories. Please delete or reassign child categories first.");
        }

        _categoryRepository.Delete(category);
        await _categoryRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category deleted successfully with ID: {CategoryId}", request.Id);

        return Result<bool>.Success(true);
    }
}
