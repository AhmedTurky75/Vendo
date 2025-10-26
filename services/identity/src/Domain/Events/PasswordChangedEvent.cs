namespace Vendo.IdentityManagement.Domain.Events;

/// <summary>
/// Domain event raised when a user's password is changed
/// </summary>
public sealed class PasswordChangedEvent : IDomainEvent
{
    public Guid UserId { get; }
    public string Username { get; }
    public DateTime OccurredOn { get; }

    public PasswordChangedEvent(Guid userId, string username)
    {
        UserId = userId;
        Username = username;
        OccurredOn = DateTime.UtcNow;
    }
}
