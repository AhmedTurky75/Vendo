using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.Catalog.Application.Common;
using Vendo.Catalog.Application.Products.DTOs;
using Vendo.Catalog.Domain.Interfaces;

namespace Vendo.Catalog.Application.Products.Commands.UpdateProduct;

/// <summary>
/// Handler for updating an existing product.
/// </summary>
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ILogger<UpdateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating product with ID: {ProductId} for tenant: {TenantId}", request.Id, request.TenantId);

        // Get existing product
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

        // Validate category exists
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            return Result<ProductDto>.Failure("Category not found");
        }

        // Check if SKU already exists for another product
        if (await _productRepository.SkuExistsAsync(request.SKU, request.TenantId, request.Id, cancellationToken))
        {
            return Result<ProductDto>.Failure($"Product with SKU '{request.SKU}' already exists");
        }

        // Update product entity
        product.CategoryId = request.CategoryId;
        product.Name = request.Name;
        product.Description = request.Description;
        product.ShortDescription = request.ShortDescription;
        product.SKU = request.SKU;
        product.Slug = GenerateSlug(request.Name);
        product.Price = request.Price;
        product.CompareAtPrice = request.CompareAtPrice;
        product.CostPrice = request.CostPrice;
        product.StockQuantity = request.StockQuantity;
        product.LowStockThreshold = request.LowStockThreshold;
        product.TrackInventory = request.TrackInventory;
        product.IsTaxable = request.IsTaxable;
        product.TaxRate = request.TaxRate;
        product.Weight = request.Weight;
        product.Dimensions = request.Dimensions;
        product.ImageUrl = request.ImageUrl;
        product.AdditionalImages = request.AdditionalImages != null ? string.Join(",", request.AdditionalImages) : null;
        product.Status = request.Status;
        product.IsFeatured = request.IsFeatured;
        product.Tags = request.Tags != null ? string.Join(",", request.Tags) : null;
        product.MetaTitle = request.MetaTitle;
        product.MetaDescription = request.MetaDescription;
        product.MetaKeywords = request.MetaKeywords;
        product.DisplayOrder = request.DisplayOrder;
        product.UpdatedAt = DateTime.UtcNow;
        product.UpdatedBy = "system"; // TODO: Get from auth context

        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product updated successfully with ID: {ProductId}", product.Id);

        // Map to DTO
        var productDto = new ProductDto
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
        };

        return Result<ProductDto>.Success(productDto);
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
