namespace Vendo.Catalog.Domain.Enums;

/// <summary>
/// Product availability status.
/// </summary>
public enum ProductStatus
{
    /// <summary>
    /// Product is in draft mode (not published).
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Product is active and available for purchase.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Product is out of stock.
    /// </summary>
    OutOfStock = 2,

    /// <summary>
    /// Product is discontinued and no longer available.
    /// </summary>
    Discontinued = 3,

    /// <summary>
    /// Product is archived (soft deleted).
    /// </summary>
    Archived = 4
}
