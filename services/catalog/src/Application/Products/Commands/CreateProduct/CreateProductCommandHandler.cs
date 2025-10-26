using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.CatalogManagement.Application.Common;
using Vendo.CatalogManagement.Application.Products.DTOs;
using Vendo.CatalogManagement.Domain.Entities;
using Vendo.CatalogManagement.Domain.Interfaces;

namespace Vendo.CatalogManagement.Application.Products.Commands.CreateProduct;

/// <summary>
/// Handler for creating a new product.
/// </summary>
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ILogger<CreateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product with SKU: {SKU} for tenant: {TenantId}", request.SKU, request.TenantId);

        // Validate category exists
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            return Result<ProductDto>.Failure("Category not found");
        }

        // Check if SKU already exists
        if (await _productRepository.SkuExistsAsync(request.SKU, request.TenantId, null, cancellationToken))
        {
            return Result<ProductDto>.Failure($"Product with SKU '{request.SKU}' already exists");
        }

        // Create product entity
        var product = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            CategoryId = request.CategoryId,
            Name = request.Name,
            Description = request.Description,
            ShortDescription = request.ShortDescription,
            SKU = request.SKU,
            Slug = GenerateSlug(request.Name),
            Price = request.Price,
            CompareAtPrice = request.CompareAtPrice,
            CostPrice = request.CostPrice,
            StockQuantity = request.StockQuantity,
            LowStockThreshold = request.LowStockThreshold,
            TrackInventory = request.TrackInventory,
            IsTaxable = request.IsTaxable,
            TaxRate = request.TaxRate,
            Weight = request.Weight,
            Dimensions = request.Dimensions,
            ImageUrl = request.ImageUrl,
            AdditionalImages = request.AdditionalImages != null ? string.Join(",", request.AdditionalImages) : null,
            Status = request.Status,
            IsFeatured = request.IsFeatured,
            Tags = request.Tags != null ? string.Join(",", request.Tags) : null,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription,
            MetaKeywords = request.MetaKeywords,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "system", // TODO: Get from auth context
            UpdatedBy = "system"
        };

        await _productRepository.AddAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);

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
