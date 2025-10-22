using MediatR;
using Vendo.TenantManagement.Application.Common.Models;
using Vendo.TenantManagement.Application.Stores.DTOs;

namespace Vendo.TenantManagement.Application.Stores.Commands.CreateStore;

/// <summary>
/// Command to create a new store.
/// </summary>
public sealed class CreateStoreCommand : IRequest<Result<StoreDto>>
{
    /// <summary>
    /// Gets or sets the store name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique subdomain.
    /// </summary>
    public string Subdomain { get; set; } = string.Empty;

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
    /// Gets or sets the owner user ID (merchant admin).
    /// </summary>
    public string OwnerId { get; set; } = string.Empty;
}
