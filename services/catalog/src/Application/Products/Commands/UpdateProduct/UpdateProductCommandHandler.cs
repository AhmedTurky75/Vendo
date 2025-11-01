using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.CatalogManagement.Application.Common;
using Vendo.CatalogManagement.Application.Products.DTOs;
using Vendo.CatalogManagement.Domain.Enums;
using Vendo.CatalogManagement.Domain.Interfaces;
using Vendo.CatalogManagement.Domain.ValueObjects;

namespace Vendo.CatalogManagement.Application.Products.Commands.UpdateProduct;

/// <summary>
/// Handler for updating an existing product using DDD domain model.
/// </summary>
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateProductCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating product with ID: {ProductId} for tenant: {TenantId}", request.Id, request.TenantId);

        // Get existing product
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            return Result<ProductDto>.Failure("Product not found");
        }

        // Verify tenant ownership
        if (product.TenantId != request.TenantId)
        {
            return Result<ProductDto>.Failure("Product does not belong to this tenant");
        }

        // Validate category exists if changing
        if (product.CategoryId != request.CategoryId)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
            {
                return Result<ProductDto>.Failure("Category not found");
            }

            product.ChangeCategory(request.CategoryId, "system");
        }

        // Get category for DTO mapping
        var currentCategory = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId, cancellationToken);

        // Validate SKU format
        var newSku = SKU.Create(request.SKU);
        if (newSku == null)
        {
            return Result<ProductDto>.Failure($"Invalid SKU format: '{request.SKU}'");
        }

        // Check if SKU already exists for another product
        if (product.SKU.Value != newSku.Value)
        {
            if (await _unitOfWork.Products.SkuExistsAsync(newSku.Value, request.TenantId, request.Id, cancellationToken))
            {
                return Result<ProductDto>.Failure($"Product with SKU '{request.SKU}' already exists");
            }
        }

        // Update basic information
        product.UpdateInformation(request.Name, request.Description, request.ShortDescription, "system");

        // Update price
        var newPrice = Money.Create(request.Price);
        if (newPrice == null)
        {
            return Result<ProductDto>.Failure("Price must be a positive value");
        }

        if (product.Price.Amount != newPrice.Amount)
        {
            product.ChangePrice(newPrice, "system");
        }

        // Update compare at price
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
        else
        {
            product.SetCompareAtPrice(null, "system");
        }

        // Update cost price
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
        else
        {
            product.SetCostPrice(null, "system");
        }

        // Update stock
        if (product.StockQuantity != request.StockQuantity)
        {
            product.UpdateStock(request.StockQuantity, "system");
        }

        product.SetLowStockThreshold(request.LowStockThreshold, "system");
        product.SetInventoryTracking(request.TrackInventory, "system");

        // Update tax configuration
        product.SetTaxConfiguration(request.IsTaxable, request.TaxRate, "system");

        // Update weight
        product.SetWeight(request.Weight, "system");

        // Update dimensions
        if (!string.IsNullOrWhiteSpace(request.Dimensions))
        {
            var dimensions = Dimensions.Parse(request.Dimensions);
            product.SetDimensions(dimensions, "system");
        }
        else
        {
            product.SetDimensions(null, "system");
        }

        // Update images
        var images = ProductImages.Create(request.ImageUrl, request.AdditionalImages);
        product.SetImages(images, "system");

        // Update SEO metadata
        var seoMetadata = SEOMetadata.Create(request.MetaTitle, request.MetaDescription, request.MetaKeywords);
        product.SetSEOMetadata(seoMetadata, "system");

        // Update status
        var currentStatus = product.Status;
        if (currentStatus != request.Status)
        {
            try
            {
                switch (request.Status)
                {
                    case ProductStatus.Active:
                        product.Publish("system");
                        break;
                    case ProductStatus.Inactive:
                        product.Unpublish("system");
                        break;
                    case ProductStatus.Draft:
                        product.SetDraft("system");
                        break;
                    case ProductStatus.Archived:
                        product.Archive("system");
                        break;
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot change product status to {Status}", request.Status);
            }
        }

        // Update featured flag
        product.SetFeatured(request.IsFeatured, "system");

        // Update tags - remove all and add new ones
        var existingTags = product.Tags.ToList();
        foreach (var tag in existingTags)
        {
            product.RemoveTag(tag, "system");
        }

        if (request.Tags != null)
        {
            foreach (var tag in request.Tags)
            {
                product.AddTag(tag, "system");
            }
        }

        // Update display order
        product.SetDisplayOrder(request.DisplayOrder, "system");

        // Mark as updated
        _unitOfWork.Products.Update(product);

        // Save changes (will dispatch domain events)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product updated successfully with ID: {ProductId}", product.Id);

        // Map to DTO
        var productDto = MapToDto(product, currentCategory!);

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
