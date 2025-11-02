using Vendo.CatalogManagement.Domain.Events;

namespace Vendo.CatalogManagement.Domain.Common;

/// <summary>
/// Base entity class with common properties and domain event support.
/// </summary>
public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Gets the unique identifier.
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Gets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Gets the last update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; protected set; }

    /// <summary>
    /// Gets the user who created this entity.
    /// </summary>
    public string CreatedBy { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the user who last updated this entity.
    /// </summary>
    public string UpdatedBy { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the domain events raised by this entity.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to be dispatched.
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Sets the audit fields for creation.
    /// </summary>
    protected void SetCreatedAudit(string createdBy)
    {
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = createdBy;
    }

    /// <summary>
    /// Sets the audit fields for update.
    /// </summary>
    protected void SetUpdatedAudit(string updatedBy)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}
