using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.Catalog.Application.Categories.DTOs;
using Vendo.Catalog.Application.Common;
using Vendo.Catalog.Domain.Interfaces;

namespace Vendo.Catalog.Application.Categories.Queries.GetCategory;

/// <summary>
/// Handler for getting a category by ID.
/// </summary>
public class GetCategoryQueryHandler : IRequestHandler<GetCategoryQuery, Result<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<GetCategoryQueryHandler> _logger;

    public GetCategoryQueryHandler(
        ICategoryRepository categoryRepository,
        ILogger<GetCategoryQueryHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Result<CategoryDto>> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting category with ID: {CategoryId} for tenant: {TenantId}", request.Id, request.TenantId);

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

        var categoryDto = new CategoryDto
        {
            Id = category.Id,
            TenantId = category.TenantId,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.Name,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ImageUrl = category.ImageUrl,
            ProductCount = category.Products?.Count ?? 0,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };

        return Result<CategoryDto>.Success(categoryDto);
    }
}
