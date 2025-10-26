using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.CatalogManagement.Application.Categories.DTOs;
using Vendo.CatalogManagement.Application.Common;
using Vendo.CatalogManagement.Domain.Interfaces;

namespace Vendo.CatalogManagement.Application.Categories.Queries.GetCategories;

/// <summary>
/// Handler for getting all categories for a tenant.
/// </summary>
public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<GetCategoriesQueryHandler> _logger;

    public GetCategoriesQueryHandler(
        ICategoryRepository categoryRepository,
        ILogger<GetCategoriesQueryHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting categories for tenant: {TenantId}, OnlyRoot: {OnlyRoot}",
            request.TenantId, request.OnlyRoot);

        var categories = request.OnlyRoot == true
            ? await _categoryRepository.GetRootCategoriesAsync(request.TenantId, cancellationToken)
            : await _categoryRepository.GetAllAsync(request.TenantId, cancellationToken);

        var categoryDtos = categories.Select(category => new CategoryDto
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
        }).ToList();

        return Result<List<CategoryDto>>.Success(categoryDtos);
    }
}
