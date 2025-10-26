using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.CatalogManagement.Application.Categories.DTOs;
using Vendo.CatalogManagement.Application.Common;
using Vendo.CatalogManagement.Domain.Interfaces;

namespace Vendo.CatalogManagement.Application.Categories.Commands.UpdateCategory;

/// <summary>
/// Handler for updating an existing category.
/// </summary>
public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        ILogger<UpdateCategoryCommandHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating category with ID: {CategoryId} for tenant: {TenantId}", request.Id, request.TenantId);

        // Get existing category
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category == null)
        {
            return Result<CategoryDto>.Failure("Category not found");
        }

        // Verify tenant ownership
        if (category.TenantId != request.TenantId)
        {
            return Result<CategoryDto>.Failure("Category does not belong to this tenant");
        }

        // Validate parent category if specified
        if (request.ParentCategoryId.HasValue)
        {
            // Prevent circular reference
            if (request.ParentCategoryId.Value == request.Id)
            {
                return Result<CategoryDto>.Failure("Category cannot be its own parent");
            }

            var parentCategory = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken);
            if (parentCategory == null)
            {
                return Result<CategoryDto>.Failure("Parent category not found");
            }

            if (parentCategory.TenantId != request.TenantId)
            {
                return Result<CategoryDto>.Failure("Parent category does not belong to this tenant");
            }
        }

        // Generate slug
        var slug = GenerateSlug(request.Name);

        // Check if slug already exists for another category
        if (await _categoryRepository.SlugExistsAsync(slug, request.TenantId, request.Id, cancellationToken))
        {
            return Result<CategoryDto>.Failure($"Category with slug '{slug}' already exists");
        }

        // Update category entity
        category.Name = request.Name;
        category.Description = request.Description;
        category.Slug = slug;
        category.ParentCategoryId = request.ParentCategoryId;
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;
        category.ImageUrl = request.ImageUrl;
        category.UpdatedAt = DateTime.UtcNow;
        category.UpdatedBy = "system"; // TODO: Get from auth context

        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category updated successfully with ID: {CategoryId}", category.Id);

        // Map to DTO
        var categoryDto = new CategoryDto
        {
            Id = category.Id,
            TenantId = category.TenantId,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug,
            ParentCategoryId = category.ParentCategoryId,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ImageUrl = category.ImageUrl,
            ProductCount = category.Products?.Count ?? 0,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };

        return Result<CategoryDto>.Success(categoryDto);
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Replace("'", "")
            .Replace("\"", "");
    }
}
