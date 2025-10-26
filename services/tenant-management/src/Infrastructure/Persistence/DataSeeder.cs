using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Vendo.TenantManagement.Domain.Entities;
using Vendo.TenantManagement.Domain.Enums;
using Vendo.TenantManagement.Domain.ValueObjects;

namespace Vendo.TenantManagement.Infrastructure.Persistence;

/// <summary>
/// Seeds initial data into the Tenant Management database from JSON files.
/// </summary>
public class DataSeeder
{
    private readonly TenantManagementDbContext _context;
    private readonly ILogger<DataSeeder> _logger;
    private readonly string _dataPath;

    public DataSeeder(TenantManagementDbContext context, ILogger<DataSeeder> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;

        // Get the data directory path from configuration
        var configuredPath = configuration["SeedDataSettings:DataPath"] ?? "Infrastructure/Persistence/SeedData";
        var baseDirectory = AppContext.BaseDirectory;

        // Navigate up from bin/Debug/net9.0 to the src folder, then to the configured path
        var srcRoot = Path.Combine(baseDirectory, "..", "..", "..", "..");
        _dataPath = Path.GetFullPath(Path.Combine(srcRoot, configuredPath));
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting data seeding for Tenant Management service...");

            await SeedStoresAsync();

            _logger.LogInformation("Data seeding completed successfully for Tenant Management service.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the Tenant Management database.");
            throw;
        }
    }

    private async Task SeedStoresAsync()
    {
        // Check if stores already exist
        if (await _context.Stores.AnyAsync())
        {
            _logger.LogInformation("Stores already exist. Skipping store seeding.");
            return;
        }

        var storesFilePath = Path.Combine(_dataPath, "stores.json");

        if (!File.Exists(storesFilePath))
        {
            _logger.LogWarning($"Stores seed file not found at {storesFilePath}");
            return;
        }

        _logger.LogInformation($"Reading stores from {storesFilePath}");
        var storesJson = await File.ReadAllTextAsync(storesFilePath);
        var storeDtos = JsonSerializer.Deserialize<List<StoreSeedDto>>(storesJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (storeDtos == null || !storeDtos.Any())
        {
            _logger.LogWarning("No stores found in seed file.");
            return;
        }

        var stores = new List<Store>();
        foreach (var dto in storeDtos)
        {
            // Create Subdomain
            var subdomainResult = Subdomain.Create(dto.Subdomain);
            if (!subdomainResult.IsSuccess)
            {
                _logger.LogWarning($"Invalid subdomain '{dto.Subdomain}': {subdomainResult.Error}");
                continue;
            }

            // Create MerchantInfo
            var merchantInfo = MerchantInfo.Create(
                dto.MerchantInfo.Email,
                dto.MerchantInfo.Phone,
                dto.MerchantInfo.BusinessName,
                dto.MerchantInfo.Address,
                dto.MerchantInfo.City,
                dto.MerchantInfo.State,
                dto.MerchantInfo.PostalCode,
                dto.MerchantInfo.Country
            );

            // Create Store
            var store = Store.Create(
                dto.Name,
                subdomainResult.Value!,
                merchantInfo,
                dto.OwnerId
            );

            // Set additional properties using reflection
            typeof(Store).GetProperty("Id")!.SetValue(store, dto.Id);
            typeof(Store).GetProperty("Status")!.SetValue(store, dto.Status);
            typeof(Store).GetProperty("SubscriptionTier")!.SetValue(store, dto.SubscriptionTier);
            typeof(Store).GetProperty("CreatedAt")!.SetValue(store, dto.CreatedAt);
            typeof(Store).GetProperty("UpdatedAt")!.SetValue(store, dto.UpdatedAt);

            // Create and set StoreSettings
            var settings = StoreSettings.Create(
                dto.Settings.Currency,
                dto.Settings.Timezone,
                dto.Settings.Language,
                dto.Settings.TaxRate,
                dto.Settings.TaxEnabled,
                dto.Settings.PrimaryColor,
                dto.Settings.AccentColor,
                dto.Settings.LogoUrl
            );
            store.UpdateSettings(settings);

            stores.Add(store);
        }

        await _context.Stores.AddRangeAsync(stores);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Seeded {stores.Count} stores successfully.");
    }
}

/// <summary>
/// DTO for deserializing store seed data from JSON.
/// </summary>
internal class StoreSeedDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public StoreStatus Status { get; set; }
    public SubscriptionTier SubscriptionTier { get; set; }
    public MerchantInfoDto MerchantInfo { get; set; } = new();
    public StoreSettingsDto Settings { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for merchant info within store seed data.
/// </summary>
internal class MerchantInfoDto
{
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? BusinessName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
}

/// <summary>
/// DTO for store settings within store seed data.
/// </summary>
internal class StoreSettingsDto
{
    public string Currency { get; set; } = "USD";
    public string Timezone { get; set; } = "America/New_York";
    public string Language { get; set; } = "en";
    public decimal TaxRate { get; set; }
    public bool TaxEnabled { get; set; }
    public string? PrimaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? LogoUrl { get; set; }
}
