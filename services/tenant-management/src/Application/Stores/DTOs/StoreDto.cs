namespace Vendo.TenantManagement.Application.Stores.DTOs;

/// <summary>
/// Data transfer object for Store entity.
/// </summary>
public class StoreDto
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
    /// Gets or sets the subdomain.
    /// </summary>
    public string Subdomain { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the store status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subscription tier.
    /// </summary>
    public string SubscriptionTier { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owner user ID.
    /// </summary>
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the merchant email.
    /// </summary>
    public string MerchantEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the merchant phone.
    /// </summary>
    public string? MerchantPhone { get; set; }

    /// <summary>
    /// Gets or sets the merchant business name.
    /// </summary>
    public string? MerchantBusinessName { get; set; }

    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timezone.
    /// </summary>
    public string Timezone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the store is in trial period.
    /// </summary>
    public bool IsInTrial { get; set; }

    /// <summary>
    /// Gets or sets when the trial ends.
    /// </summary>
    public DateTime? TrialEndsAt { get; set; }

    /// <summary>
    /// Gets or sets when the store was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the store was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
