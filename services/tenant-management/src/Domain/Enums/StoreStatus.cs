namespace Vendo.TenantManagement.Domain.Enums;

/// <summary>
/// Represents the operational status of a store.
/// </summary>
public enum StoreStatus
{
    /// <summary>
    /// Store is active and operational.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Store is temporarily inactive.
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Store is suspended due to policy violation or payment issues.
    /// </summary>
    Suspended = 3,

    /// <summary>
    /// Store is archived and no longer operational.
    /// </summary>
    Archived = 4
}
