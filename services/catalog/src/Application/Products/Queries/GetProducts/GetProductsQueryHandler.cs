using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.CatalogManagement.Application.Common;
using Vendo.CatalogManagement.Application.Products.DTOs;
using Vendo.CatalogManagement.Domain.Interfaces;

namespace Vendo.CatalogManagement.Application.Products.Queries.GetProducts;

/// <summary>
/// Handler for getting all products with pagination.
/// </summary>
public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<PagedResult<ProductDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<GetProductsQueryHandler> _logger;

    public GetProductsQueryHandler(
        IProductRepository productRepository,
        ILogger<GetProductsQueryHandler> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting products for tenant: {TenantId}, Page: {PageNumber}, PageSize: {PageSize}",
            request.TenantId, request.PageNumber, request.PageSize);

        var skip = (request.PageNumber - 1) * request.PageSize;
        var products = await _productRepository.GetAllAsync(request.TenantId, skip, request.PageSize, cancellationToken);
        var totalCount = await _productRepository.GetCountAsync(request.TenantId, cancellationToken);

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
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PagedResult<ProductDto>>.Success(pagedResult);
    }
}
