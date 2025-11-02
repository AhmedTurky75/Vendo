namespace Vendo.CatalogManagement.Domain.Events;

/// <summary>
/// Domain event raised when a product's stock quantity changes.
/// </summary>
public sealed class ProductStockChangedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
    public Guid ProductId { get; }
    public int OldQuantity { get; }
    public int NewQuantity { get; }
    public bool IsLowStock { get; }

    public ProductStockChangedEvent(
        Guid productId,
        int oldQuantity,
        int newQuantity,
        bool isLowStock)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        ProductId = productId;
        OldQuantity = oldQuantity;
        NewQuantity = newQuantity;
        IsLowStock = isLowStock;
    }
}
