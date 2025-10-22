using Vendo.Identity.Domain.Events;
using Vendo.Identity.Domain.ValueObjects;

namespace Vendo.Identity.Domain.Entities;

/// <summary>
/// Represents a user in the identity system
/// </summary>
public class User
{
    private readonly List<string> _roles = new();
    private readonly List<IDomainEvent> _domainEvents = new();

    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public IReadOnlyList<string> Roles => _roles.AsReadOnly();
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private User() { }

    public static User Create(
        string username,
        Email email,
        string passwordHash,
        string firstName,
        string lastName,
        List<string>? roles = null)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (roles != null && roles.Any())
        {
            user._roles.AddRange(roles);
        }

        user.AddDomainEvent(new UserRegisteredEvent(user.Id, user.Username, user.Email.Value));

        return user;
    }

    public void UpdateProfile(string firstName, string lastName, Email email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserUpdatedEvent(Id, firstName, lastName, email.Value));
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PasswordChangedEvent(Id, Username));
    }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void AddRole(string role)
    {
        if (!_roles.Contains(role))
        {
            _roles.Add(role);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveRole(string role)
    {
        if (_roles.Contains(role))
        {
            _roles.Remove(role);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
