namespace Vendo.CatalogManagement.Domain.Events;

/// <summary>
/// Domain event raised when a new product is created.
/// </summary>
public sealed class ProductCreatedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
    public Guid ProductId { get; }
    public Guid TenantId { get; }
    public Guid CategoryId { get; }
    public string ProductName { get; }
    public string SKU { get; }

    public ProductCreatedEvent(
        Guid productId,
        Guid tenantId,
        Guid categoryId,
        string productName,
        string sku)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        ProductId = productId;
        TenantId = tenantId;
        CategoryId = categoryId;
        ProductName = productName;
        SKU = sku;
    }
}
