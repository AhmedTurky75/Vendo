using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.Catalog.Application.Common;
using Vendo.Catalog.Application.Products.DTOs;
using Vendo.Catalog.Domain.Interfaces;

namespace Vendo.Catalog.Application.Products.Queries.GetProduct;

/// <summary>
/// Handler for getting a product by ID.
/// </summary>
public class GetProductQueryHandler : IRequestHandler<GetProductQuery, Result<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<GetProductQueryHandler> _logger;

    public GetProductQueryHandler(
        IProductRepository productRepository,
        ILogger<GetProductQueryHandler> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Result<ProductDto>> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting product with ID: {ProductId} for tenant: {TenantId}", request.Id, request.TenantId);

        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product == null)
        {
            return Result<ProductDto>.Failure("Product not found");
        }

        // Verify tenant ownership
        if (product.TenantId != request.TenantId)
        {
            return Result<ProductDto>.Failure("Product does not belong to this tenant");
        }

        var productDto = new ProductDto
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
        };

        return Result<ProductDto>.Success(productDto);
    }
}
