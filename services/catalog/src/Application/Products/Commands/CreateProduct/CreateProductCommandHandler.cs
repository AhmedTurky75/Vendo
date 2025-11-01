using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.CatalogManagement.Application.Common;
using Vendo.CatalogManagement.Application.Products.DTOs;
using Vendo.CatalogManagement.Domain.Enums;
using Vendo.CatalogManagement.Domain.Interfaces;
using Vendo.CatalogManagement.Domain.ValueObjects;

namespace Vendo.CatalogManagement.Application.Products.Commands.CreateProduct;

/// <summary>
/// Handler for creating a new product using DDD domain model.
/// </summary>
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateProductCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product with SKU: {SKU} for tenant: {TenantId}", request.SKU, request.TenantId);

        // Validate category exists
        var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            return Result<ProductDto>.Failure("Category not found");
        }

        // Create SKU value object
        var sku = SKU.Create(request.SKU);
        if (sku == null)
        {
            return Result<ProductDto>.Failure($"Invalid SKU format. SKU must be 3-50 alphanumeric characters: '{request.SKU}'");
        }

        // Check if SKU already exists
        if (await _unitOfWork.Products.SkuExistsAsync(sku.Value, request.TenantId, null, cancellationToken))
        {
            return Result<ProductDto>.Failure($"Product with SKU '{request.SKU}' already exists");
        }

        // Create Price value object
        var price = Money.Create(request.Price);
        if (price == null)
        {
            return Result<ProductDto>.Failure("Price must be a positive value");
        }

        // Use factory method to create product with required fields
        var product = Domain.Entities.Product.Create(
            request.TenantId,
            request.CategoryId,
            request.Name,
            request.Description,
            sku,
            price,
            "system" // TODO: Get from auth context
        );

        // Set short description
        if (!string.IsNullOrWhiteSpace(request.ShortDescription))
        {
            product.UpdateInformation(request.Name, request.Description, request.ShortDescription, "system");
        }

        // Set compare at price (discount price)
        if (request.CompareAtPrice.HasValue)
        {
            var compareAtPrice = Money.Create(request.CompareAtPrice.Value);
            if (compareAtPrice != null)
            {
                try
                {
                    product.SetCompareAtPrice(compareAtPrice, "system");
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "Invalid compare at price");
                }
            }
        }

        // Set cost price
        if (request.CostPrice.HasValue)
        {
            var costPrice = Money.Create(request.CostPrice.Value);
            if (costPrice != null)
            {
                try
                {
                    product.SetCostPrice(costPrice, "system");
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "Invalid cost price");
                }
            }
        }

        // Set stock quantity
        product.UpdateStock(request.StockQuantity, "system");
        product.SetLowStockThreshold(request.LowStockThreshold, "system");
        product.SetInventoryTracking(request.TrackInventory, "system");

        // Set tax configuration
        product.SetTaxConfiguration(request.IsTaxable, request.TaxRate, "system");

        // Set weight
        if (request.Weight.HasValue)
        {
            product.SetWeight(request.Weight.Value, "system");
        }

        // Set dimensions
        if (!string.IsNullOrWhiteSpace(request.Dimensions))
        {
            var dimensions = Dimensions.Parse(request.Dimensions);
            if (dimensions != null)
            {
                product.SetDimensions(dimensions, "system");
            }
        }

        // Set images
        var images = ProductImages.Create(request.ImageUrl, request.AdditionalImages);
        product.SetImages(images, "system");

        // Set SEO metadata
        var seoMetadata = SEOMetadata.Create(request.MetaTitle, request.MetaDescription, request.MetaKeywords);
        product.SetSEOMetadata(seoMetadata, "system");

        // Set status
        if (request.Status == ProductStatus.Active)
        {
            try
            {
                product.Publish("system");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot publish product on creation");
            }
        }

        // Set featured flag
        product.SetFeatured(request.IsFeatured, "system");

        // Add tags
        if (request.Tags != null)
        {
            foreach (var tag in request.Tags)
            {
                product.AddTag(tag, "system");
            }
        }

        // Set display order
        product.SetDisplayOrder(request.DisplayOrder, "system");

        // Add to repository
        await _unitOfWork.Products.AddAsync(product, cancellationToken);

        // Save changes (will dispatch domain events)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);

        // Map to DTO
        var productDto = MapToDto(product, category);

        return Result<ProductDto>.Success(productDto);
    }

    private static ProductDto MapToDto(Domain.Entities.Product product, Domain.Entities.Category category)
    {
        return new ProductDto
        {
            Id = product.Id,
            TenantId = product.TenantId,
            CategoryId = product.CategoryId,
            CategoryName = category.Name,
            Name = product.Name,
            Description = product.Description,
            ShortDescription = product.ShortDescription,
            SKU = product.SKU.Value,
            Slug = product.Slug.Value,
            Price = product.Price.Amount,
            CompareAtPrice = product.CompareAtPrice?.Amount,
            CostPrice = product.CostPrice?.Amount,
            StockQuantity = product.StockQuantity,
            LowStockThreshold = product.LowStockThreshold,
            TrackInventory = product.TrackInventory,
            IsTaxable = product.IsTaxable,
            TaxRate = product.TaxRate,
            Weight = product.Weight,
            Dimensions = product.Dimensions?.ToString(),
            ImageUrl = product.Images.MainImageUrl,
            AdditionalImages = product.Images.AdditionalImageUrls.ToList(),
            Status = product.Status,
            IsFeatured = product.IsFeatured,
            Tags = product.Tags.ToList(),
            MetaTitle = product.SEOMetadata.MetaTitle,
            MetaDescription = product.SEOMetadata.MetaDescription,
            MetaKeywords = product.SEOMetadata.MetaKeywords,
            DisplayOrder = product.DisplayOrder,
            ViewCount = product.ViewCount,
            SalesCount = product.SalesCount,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
