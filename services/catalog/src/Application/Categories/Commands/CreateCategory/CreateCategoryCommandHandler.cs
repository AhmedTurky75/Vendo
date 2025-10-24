using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.Catalog.Application.Categories.DTOs;
using Vendo.Catalog.Application.Common;
using Vendo.Catalog.Domain.Entities;
using Vendo.Catalog.Domain.Interfaces;

namespace Vendo.Catalog.Application.Categories.Commands.CreateCategory;

/// <summary>
/// Handler for creating a new category.
/// </summary>
public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CreateCategoryCommandHandler> _logger;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        ILogger<CreateCategoryCommandHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating category: {Name} for tenant: {TenantId}", request.Name, request.TenantId);

        // Validate parent category if specified
        if (request.ParentCategoryId.HasValue)
        {
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

        // Check if slug already exists
        if (await _categoryRepository.SlugExistsAsync(slug, request.TenantId, null, cancellationToken))
        {
            return Result<CategoryDto>.Failure($"Category with slug '{slug}' already exists");
        }

        // Create category entity
        var category = new Category
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Name = request.Name,
            Description = request.Description,
            Slug = slug,
            ParentCategoryId = request.ParentCategoryId,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            ImageUrl = request.ImageUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "system", // TODO: Get from auth context
            UpdatedBy = "system"
        };

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _categoryRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category created successfully with ID: {CategoryId}", category.Id);

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
            ProductCount = 0,
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
