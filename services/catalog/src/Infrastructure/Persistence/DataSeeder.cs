using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Vendo.Catalog.Domain.Entities;
using Vendo.Catalog.Domain.Enums;

namespace Vendo.Catalog.Infrastructure.Persistence;

/// <summary>
/// Seeds initial data into the Catalog database from JSON files.
/// </summary>
public class DataSeeder
{
    private readonly CatalogDbContext _context;
    private readonly ILogger<DataSeeder> _logger;
    private readonly string _dataPath;

    public DataSeeder(CatalogDbContext context, ILogger<DataSeeder> logger)
    {
        _context = context;
        _logger = logger;

        // Get the data directory path (goes up from bin/Debug/net9.0 to project root, then to Data/SeedData)
        var assemblyLocation = AppContext.BaseDirectory;
        _dataPath = Path.Combine(assemblyLocation, "..", "..", "..", "..", "Data", "SeedData");
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting data seeding for Catalog service...");

            await SeedCategoriesAsync();
            await SeedProductsAsync();

            _logger.LogInformation("Data seeding completed successfully for Catalog service.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the Catalog database.");
            throw;
        }
    }

    private async Task SeedCategoriesAsync()
    {
        // Check if categories already exist
        if (await _context.Categories.AnyAsync())
        {
            _logger.LogInformation("Categories already exist. Skipping category seeding.");
            return;
        }

        var categoriesFilePath = Path.Combine(_dataPath, "categories.json");

        if (!File.Exists(categoriesFilePath))
        {
            _logger.LogWarning($"Categories seed file not found at {categoriesFilePath}");
            return;
        }

        _logger.LogInformation($"Reading categories from {categoriesFilePath}");
        var categoriesJson = await File.ReadAllTextAsync(categoriesFilePath);
        var categoryDtos = JsonSerializer.Deserialize<List<CategorySeedDto>>(categoriesJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (categoryDtos == null || !categoryDtos.Any())
        {
            _logger.LogWarning("No categories found in seed file.");
            return;
        }

        var categories = new List<Category>();
        foreach (var dto in categoryDtos)
        {
            var category = new Category
            {
                Id = dto.Id,
                TenantId = dto.TenantId,
                Name = dto.Name,
                Description = dto.Description,
                Slug = dto.Slug,
                ParentCategoryId = dto.ParentCategoryId,
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive,
                ImageUrl = dto.ImageUrl,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };

            categories.Add(category);
        }

        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Seeded {categories.Count} categories successfully.");
    }

    private async Task SeedProductsAsync()
    {
        // Check if products already exist
        if (await _context.Products.AnyAsync())
        {
            _logger.LogInformation("Products already exist. Skipping product seeding.");
            return;
        }

        var productsFilePath = Path.Combine(_dataPath, "products.json");

        if (!File.Exists(productsFilePath))
        {
            _logger.LogWarning($"Products seed file not found at {productsFilePath}");
            return;
        }

        _logger.LogInformation($"Reading products from {productsFilePath}");
        var productsJson = await File.ReadAllTextAsync(productsFilePath);
        var productDtos = JsonSerializer.Deserialize<List<ProductSeedDto>>(productsJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (productDtos == null || !productDtos.Any())
        {
            _logger.LogWarning("No products found in seed file.");
            return;
        }

        var products = new List<Product>();
        foreach (var dto in productDtos)
        {
            var product = new Product
            {
                Id = dto.Id,
                TenantId = dto.TenantId,
                CategoryId = dto.CategoryId,
                Name = dto.Name,
                Slug = dto.Slug,
                Description = dto.Description,
                ShortDescription = dto.ShortDescription,
                SKU = dto.SKU,
                Price = dto.Price,
                CompareAtPrice = dto.CompareAtPrice,
                CostPrice = dto.CostPrice,
                StockQuantity = dto.StockQuantity,
                LowStockThreshold = dto.LowStockThreshold,
                TrackInventory = dto.TrackInventory,
                IsTaxable = dto.IsTaxable,
                TaxRate = dto.TaxRate,
                Weight = dto.Weight,
                Dimensions = dto.Dimensions,
                ImageUrl = dto.ImageUrl,
                Status = dto.Status,
                IsFeatured = dto.IsFeatured,
                Tags = dto.Tags,
                MetaTitle = dto.MetaTitle,
                MetaDescription = dto.MetaDescription,
                DisplayOrder = dto.DisplayOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            products.Add(product);
        }

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Seeded {products.Count} products successfully.");
    }
}

/// <summary>
/// DTO for deserializing category seed data from JSON.
/// </summary>
internal class CategorySeedDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for deserializing product seed data from JSON.
/// </summary>
internal class ProductSeedDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public bool TrackInventory { get; set; }
    public bool IsTaxable { get; set; }
    public decimal TaxRate { get; set; }
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public string? ImageUrl { get; set; }
    public ProductStatus Status { get; set; }
    public bool IsFeatured { get; set; }
    public string? Tags { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public int DisplayOrder { get; set; }
}
