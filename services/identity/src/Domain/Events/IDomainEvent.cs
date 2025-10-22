namespace Vendo.Identity.Domain.Events;

/// <summary>
/// Marker interface for domain events
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
