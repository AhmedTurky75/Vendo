using Vendo.TenantManagement.Domain.Common;
using Vendo.TenantManagement.Domain.Enums;
using Vendo.TenantManagement.Domain.ValueObjects;

namespace Vendo.TenantManagement.Domain.Entities;

/// <summary>
/// Represents a merchant's store (tenant) in the multi-tenant e-commerce platform.
/// This is an aggregate root for tenant management.
/// </summary>
public class Store : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Gets or sets the store name (display name).
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the store's unique subdomain.
    /// </summary>
    public Subdomain Subdomain { get; private set; } = default!;

    /// <summary>
    /// Gets or sets the merchant information.
    /// </summary>
    public MerchantInfo MerchantInfo { get; private set; } = default!;

    /// <summary>
    /// Gets or sets the store settings.
    /// </summary>
    public StoreSettings Settings { get; private set; } = default!;

    /// <summary>
    /// Gets or sets the store's operational status.
    /// </summary>
    public StoreStatus Status { get; private set; }

    /// <summary>
    /// Gets or sets the subscription tier.
    /// </summary>
    public SubscriptionTier SubscriptionTier { get; private set; }

    /// <summary>
    /// Gets or sets the identifier of the user who owns this store (merchant admin).
    /// </summary>
    public string OwnerId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date when the trial period ends.
    /// </summary>
    public DateTime? TrialEndsAt { get; private set; }

    /// <summary>
    /// Gets or sets the date when the subscription was last renewed.
    /// </summary>
    public DateTime? SubscriptionRenewedAt { get; private set; }

    /// <summary>
    /// Gets whether the store is in trial period.
    /// </summary>
    public bool IsInTrial => TrialEndsAt.HasValue && TrialEndsAt.Value > DateTime.UtcNow;

    /// <summary>
    /// Gets whether the store is active.
    /// </summary>
    public bool IsActive => Status == StoreStatus.Active;

    // EF Core requires a parameterless constructor
    private Store() { }

    private Store(
        Guid id,
        string name,
        Subdomain subdomain,
        MerchantInfo merchantInfo,
        string ownerId,
        SubscriptionTier subscriptionTier = SubscriptionTier.Free,
        StoreStatus status = StoreStatus.Active)
    {
        Id = id;
        Name = name;
        Subdomain = subdomain;
        MerchantInfo = merchantInfo;
        OwnerId = ownerId;
        SubscriptionTier = subscriptionTier;
        Status = status;
        Settings = StoreSettings.CreateDefault();
        CreatedAt = DateTime.UtcNow;

        // Set trial period for new stores (14 days of Pro features)
        TrialEndsAt = DateTime.UtcNow.AddDays(14);
    }

    /// <summary>
    /// Creates a new store.
    /// </summary>
    /// <param name="name">The store name.</param>
    /// <param name="subdomain">The unique subdomain.</param>
    /// <param name="merchantInfo">The merchant information.</param>
    /// <param name="ownerId">The owner user ID.</param>
    /// <returns>A new Store instance.</returns>
    public static Store Create(
        string name,
        Subdomain subdomain,
        MerchantInfo merchantInfo,
        string ownerId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Store name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(ownerId))
            throw new ArgumentException("Owner ID cannot be empty", nameof(ownerId));

        return new Store(
            Guid.NewGuid(),
            name,
            subdomain,
            merchantInfo,
            ownerId
        );
    }

    /// <summary>
    /// Updates the store name.
    /// </summary>
    /// <param name="name">The new store name.</param>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Store name cannot be empty", nameof(name));

        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the merchant information.
    /// </summary>
    /// <param name="merchantInfo">The updated merchant information.</param>
    public void UpdateMerchantInfo(MerchantInfo merchantInfo)
    {
        MerchantInfo = merchantInfo ?? throw new ArgumentNullException(nameof(merchantInfo));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the store settings.
    /// </summary>
    /// <param name="settings">The updated store settings.</param>
    public void UpdateSettings(StoreSettings settings)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Changes the store's status.
    /// </summary>
    /// <param name="status">The new status.</param>
    public void ChangeStatus(StoreStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the store.
    /// </summary>
    public void Activate()
    {
        ChangeStatus(StoreStatus.Active);
    }

    /// <summary>
    /// Deactivates the store.
    /// </summary>
    public void Deactivate()
    {
        ChangeStatus(StoreStatus.Inactive);
    }

    /// <summary>
    /// Suspends the store.
    /// </summary>
    public void Suspend()
    {
        ChangeStatus(StoreStatus.Suspended);
    }

    /// <summary>
    /// Archives the store.
    /// </summary>
    public void Archive()
    {
        ChangeStatus(StoreStatus.Archived);
    }

    /// <summary>
    /// Upgrades the store to a new subscription tier.
    /// </summary>
    /// <param name="tier">The new subscription tier.</param>
    public void UpgradeSubscription(SubscriptionTier tier)
    {
        if (tier < SubscriptionTier)
            throw new InvalidOperationException("Cannot downgrade using this method. Use DowngradeSubscription instead.");

        SubscriptionTier = tier;
        SubscriptionRenewedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Downgrades the store to a new subscription tier.
    /// </summary>
    /// <param name="tier">The new subscription tier.</param>
    public void DowngradeSubscription(SubscriptionTier tier)
    {
        if (tier > SubscriptionTier)
            throw new InvalidOperationException("Cannot upgrade using this method. Use UpgradeSubscription instead.");

        SubscriptionTier = tier;
        SubscriptionRenewedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Extends the trial period.
    /// </summary>
    /// <param name="days">Number of days to extend.</param>
    public void ExtendTrial(int days)
    {
        if (days <= 0)
            throw new ArgumentException("Days must be positive", nameof(days));

        if (TrialEndsAt.HasValue)
        {
            TrialEndsAt = TrialEndsAt.Value.AddDays(days);
        }
        else
        {
            TrialEndsAt = DateTime.UtcNow.AddDays(days);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Ends the trial period.
    /// </summary>
    public void EndTrial()
    {
        TrialEndsAt = DateTime.UtcNow.AddDays(-1);
        UpdatedAt = DateTime.UtcNow;
    }
}
