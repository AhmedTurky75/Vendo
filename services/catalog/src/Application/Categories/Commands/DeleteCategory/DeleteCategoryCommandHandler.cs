using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.CatalogManagement.Application.Common;
using Vendo.CatalogManagement.Domain.Interfaces;

namespace Vendo.CatalogManagement.Application.Categories.Commands.DeleteCategory;

/// <summary>
/// Handler for deleting a category using DDD domain model.
/// </summary>
public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteCategoryCommandHandler> _logger;

    public DeleteCategoryCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteCategoryCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting category with ID: {CategoryId} for tenant: {TenantId}", request.Id, request.TenantId);

        // Get existing category
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Id, cancellationToken);
        if (category == null)
        {
            return Result<bool>.Failure("Category not found");
        }

        // Verify tenant ownership
        if (category.TenantId != request.TenantId)
        {
            return Result<bool>.Failure("Category does not belong to this tenant");
        }

        // Use domain method to validate if can be deleted
        if (!category.CanBeDeleted())
        {
            return Result<bool>.Failure("Category cannot be deleted because it has products or child categories");
        }

        _unitOfWork.Categories.Delete(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category deleted successfully with ID: {CategoryId}", request.Id);

        return Result<bool>.Success(true);
    }
}
