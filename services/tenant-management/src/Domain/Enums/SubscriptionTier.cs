namespace Vendo.TenantManagement.Domain.Enums;

/// <summary>
/// Represents the subscription tier levels available for stores.
/// </summary>
public enum SubscriptionTier
{
    /// <summary>
    /// Free tier with basic features and limitations.
    /// Limits: 50 products, 100 orders/month, basic analytics, 1 admin user
    /// </summary>
    Free = 1,

    /// <summary>
    /// Starter tier ($29/month) with enhanced features.
    /// Limits: 500 products, 1000 orders/month, advanced analytics, 3 admin users
    /// </summary>
    Starter = 2,

    /// <summary>
    /// Pro tier ($79/month) with all features and unlimited usage.
    /// Limits: Unlimited products, unlimited orders, all features, 10 admin users
    /// </summary>
    Pro = 3
}
