using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.Catalog.Application.Common;
using Vendo.Catalog.Application.Products.DTOs;
using Vendo.Catalog.Domain.Interfaces;

namespace Vendo.Catalog.Application.Products.Queries.SearchProducts;

/// <summary>
/// Handler for searching products by name or description.
/// </summary>
public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, Result<PagedResult<ProductDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<SearchProductsQueryHandler> _logger;

    public SearchProductsQueryHandler(
        IProductRepository productRepository,
        ILogger<SearchProductsQueryHandler> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching products for tenant: {TenantId}, SearchTerm: {SearchTerm}, Page: {PageNumber}",
            request.TenantId, request.SearchTerm, request.PageNumber);

        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            return Result<PagedResult<ProductDto>>.Failure("Search term is required");
        }

        var skip = (request.PageNumber - 1) * request.PageSize;
        var products = await _productRepository.SearchAsync(request.TenantId, request.SearchTerm, skip, request.PageSize, cancellationToken);

        var productDtos = products.Select(product => new ProductDto
        {
            Id = product.Id,
            TenantId = product.TenantId,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
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
        }).ToList();

        var pagedResult = new PagedResult<ProductDto>
        {
            Items = productDtos,
            TotalCount = products.Count,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PagedResult<ProductDto>>.Success(pagedResult);
    }
}
