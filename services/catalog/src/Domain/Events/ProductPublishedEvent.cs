namespace Vendo.CatalogManagement.Domain.Events;

/// <summary>
/// Domain event raised when a product is published (made active/visible).
/// </summary>
public sealed class ProductPublishedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
    public Guid ProductId { get; }
    public Guid TenantId { get; }
    public string ProductName { get; }

    public ProductPublishedEvent(
        Guid productId,
        Guid tenantId,
        string productName)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        ProductId = productId;
        TenantId = tenantId;
        ProductName = productName;
    }
}
