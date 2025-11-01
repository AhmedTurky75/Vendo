namespace Vendo.CatalogManagement.Domain.Events;

/// <summary>
/// Domain event raised when a product's price is changed.
/// </summary>
public sealed class ProductPriceChangedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
    public Guid ProductId { get; }
    public decimal OldPrice { get; }
    public decimal NewPrice { get; }
    public string Currency { get; }

    public ProductPriceChangedEvent(
        Guid productId,
        decimal oldPrice,
        decimal newPrice,
        string currency = "USD")
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        ProductId = productId;
        OldPrice = oldPrice;
        NewPrice = newPrice;
        Currency = currency;
    }
}
