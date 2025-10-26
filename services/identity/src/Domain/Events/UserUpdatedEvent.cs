namespace Vendo.IdentityManagement.Domain.Events;

/// <summary>
/// Domain event raised when a user profile is updated
/// </summary>
public sealed class UserUpdatedEvent : IDomainEvent
{
    public Guid UserId { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public DateTime OccurredOn { get; }

    public UserUpdatedEvent(Guid userId, string firstName, string lastName, string email)
    {
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        OccurredOn = DateTime.UtcNow;
    }
}
