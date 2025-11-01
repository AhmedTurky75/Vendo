using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.CatalogManagement.Application.Categories.DTOs;
using Vendo.CatalogManagement.Application.Common;
using Vendo.CatalogManagement.Domain.Interfaces;

namespace Vendo.CatalogManagement.Application.Categories.Commands.UpdateCategory;

/// <summary>
/// Handler for updating an existing category using DDD domain model.
/// </summary>
public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;

    public UpdateCategoryCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateCategoryCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating category with ID: {CategoryId} for tenant: {TenantId}", request.Id, request.TenantId);

        // Get existing category
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Id, cancellationToken);
        if (category == null)
        {
            return Result<CategoryDto>.Failure("Category not found");
        }

        // Verify tenant ownership
        if (category.TenantId != request.TenantId)
        {
            return Result<CategoryDto>.Failure("Category does not belong to this tenant");
        }

        // Validate parent category if changing
        if (request.ParentCategoryId.HasValue)
        {
            var parentCategory = await _unitOfWork.Categories.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken);
            if (parentCategory == null)
            {
                return Result<CategoryDto>.Failure("Parent category not found");
            }

            if (parentCategory.TenantId != request.TenantId)
            {
                return Result<CategoryDto>.Failure("Parent category does not belong to this tenant");
            }
        }

        // Update information
        category.UpdateInformation(request.Name, request.Description, "system");

        // Change parent if needed
        if (category.ParentCategoryId != request.ParentCategoryId)
        {
            try
            {
                category.ChangeParent(request.ParentCategoryId, "system");
            }
            catch (InvalidOperationException ex)
            {
                return Result<CategoryDto>.Failure(ex.Message);
            }
        }

        // Update active status
        if (request.IsActive && !category.IsActive)
        {
            category.Activate("system");
        }
        else if (!request.IsActive && category.IsActive)
        {
            category.Deactivate("system");
        }

        // Update image
        try
        {
            category.SetImage(request.ImageUrl, "system");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid image URL");
        }

        // Update display order
        category.SetDisplayOrder(request.DisplayOrder, "system");

        // Mark as updated
        _unitOfWork.Categories.Update(category);

        // Save changes (will dispatch domain events)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category updated successfully with ID: {CategoryId}", category.Id);

        // Map to DTO
        var categoryDto = new CategoryDto
        {
            Id = category.Id,
            TenantId = category.TenantId,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug.Value,
            ParentCategoryId = category.ParentCategoryId,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ImageUrl = category.ImageUrl,
            ProductCount = category.Products.Count,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };

        return Result<CategoryDto>.Success(categoryDto);
    }
}
