using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.CatalogManagement.Application.Categories.DTOs;
using Vendo.CatalogManagement.Application.Common;
using Vendo.CatalogManagement.Application.Products.DTOs;
using Vendo.CatalogManagement.Domain.Interfaces;

namespace Vendo.CatalogManagement.Application.Categories.Queries.GetCategoryWithProducts;

/// <summary>
/// Handler for getting a category with its products.
/// </summary>
public class GetCategoryWithProductsQueryHandler : IRequestHandler<GetCategoryWithProductsQuery, Result<CategoryWithProductsDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<GetCategoryWithProductsQueryHandler> _logger;

    public GetCategoryWithProductsQueryHandler(
        ICategoryRepository categoryRepository,
        ILogger<GetCategoryWithProductsQueryHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Result<CategoryWithProductsDto>> Handle(GetCategoryWithProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting category with products, ID: {CategoryId} for tenant: {TenantId}",
            request.Id, request.TenantId);

        var category = await _categoryRepository.GetWithProductsAsync(request.Id, cancellationToken);

        if (category == null)
        {
            return Result<CategoryWithProductsDto>.Failure("Category not found");
        }

        // Verify tenant ownership
        if (category.TenantId != request.TenantId)
        {
            return Result<CategoryWithProductsDto>.Failure("Category does not belong to this tenant");
        }

        var categoryDto = new CategoryWithProductsDto
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
            UpdatedAt = category.UpdatedAt,
            Products = category.Products?.Select(product => new ProductDto
            {
                Id = product.Id,
                TenantId = product.TenantId,
                CategoryId = product.CategoryId,
                CategoryName = category.Name,
                Name = product.Name,
                Description = product.Description,
                ShortDescription = product.ShortDescription,
                SKU = product.SKU,
                Slug = product.Slug,
                Price = product.Price,
                CompareAtPrice = product.CompareAtPrice,
                CostPrice = product.CostPrice,
                StockQuantity = product.StockQuantity,
                LowStockThreshold = product.LowStockThreshold,
                TrackInventory = product.TrackInventory,
                IsTaxable = product.IsTaxable,
                TaxRate = product.TaxRate,
                Weight = product.Weight,
                Dimensions = product.Dimensions,
                ImageUrl = product.ImageUrl,
                AdditionalImages = product.AdditionalImages?.Split(',').ToList(),
                Status = product.Status,
                IsFeatured = product.IsFeatured,
                Tags = product.Tags?.Split(',').ToList(),
                MetaTitle = product.MetaTitle,
                MetaDescription = product.MetaDescription,
                MetaKeywords = product.MetaKeywords,
                DisplayOrder = product.DisplayOrder,
                ViewCount = product.ViewCount,
                SalesCount = product.SalesCount,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            }).ToList() ?? new List<ProductDto>()
        };

        return Result<CategoryWithProductsDto>.Success(categoryDto);
    }
}
