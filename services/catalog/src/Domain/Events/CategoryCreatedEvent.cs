namespace Vendo.CatalogManagement.Domain.Events;

/// <summary>
/// Domain event raised when a new category is created.
/// </summary>
public sealed class CategoryCreatedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
    public Guid CategoryId { get; }
    public Guid TenantId { get; }
    public string CategoryName { get; }
    public Guid? ParentCategoryId { get; }

    public CategoryCreatedEvent(
        Guid categoryId,
        Guid tenantId,
        string categoryName,
        Guid? parentCategoryId)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        CategoryId = categoryId;
        TenantId = tenantId;
        CategoryName = categoryName;
        ParentCategoryId = parentCategoryId;
    }
}
