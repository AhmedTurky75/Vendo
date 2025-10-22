using MediatR;
using Vendo.TenantManagement.Application.Common.Models;
using Vendo.TenantManagement.Application.Stores.DTOs;

namespace Vendo.TenantManagement.Application.Stores.Commands.UpdateStore;

/// <summary>
/// Command to update an existing store.
/// </summary>
public sealed class UpdateStoreCommand : IRequest<Result<StoreDto>>
{
    /// <summary>
    /// Gets or sets the store ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the store name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the merchant email.
    /// </summary>
    public string MerchantEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the merchant phone number (optional).
    /// </summary>
    public string? MerchantPhone { get; set; }

    /// <summary>
    /// Gets or sets the merchant business name (optional).
    /// </summary>
    public string? MerchantBusinessName { get; set; }

    /// <summary>
    /// Gets or sets the merchant address (optional).
    /// </summary>
    public string? MerchantAddress { get; set; }

    /// <summary>
    /// Gets or sets the merchant city (optional).
    /// </summary>
    public string? MerchantCity { get; set; }

    /// <summary>
    /// Gets or sets the merchant state (optional).
    /// </summary>
    public string? MerchantState { get; set; }

    /// <summary>
    /// Gets or sets the merchant postal code (optional).
    /// </summary>
    public string? MerchantPostalCode { get; set; }

    /// <summary>
    /// Gets or sets the merchant country (optional).
    /// </summary>
    public string? MerchantCountry { get; set; }

    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Gets or sets the timezone.
    /// </summary>
    public string? Timezone { get; set; }

    /// <summary>
    /// Gets or sets the language.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Gets or sets the tax rate.
    /// </summary>
    public decimal? TaxRate { get; set; }

    /// <summary>
    /// Gets or sets whether tax is enabled.
    /// </summary>
    public bool? TaxEnabled { get; set; }

    /// <summary>
    /// Gets or sets the primary brand color.
    /// </summary>
    public string? PrimaryColor { get; set; }

    /// <summary>
    /// Gets or sets the accent color.
    /// </summary>
    public string? AccentColor { get; set; }

    /// <summary>
    /// Gets or sets the logo URL.
    /// </summary>
    public string? LogoUrl { get; set; }
}
