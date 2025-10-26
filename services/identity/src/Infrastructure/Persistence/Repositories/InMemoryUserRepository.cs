using System.Collections.Concurrent;
using Vendo.IdentityManagement.Domain.Entities;
using Vendo.IdentityManagement.Domain.Repositories;
using Vendo.IdentityManagement.Domain.ValueObjects;

namespace Vendo.IdentityManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// In-memory implementation of IUserRepository for development/testing
/// </summary>
public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = _users.Values.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        var user = _users.Values.FirstOrDefault(u => u.Email.Equals(email));
        return Task.FromResult(user);
    }

    public Task<List<User>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _users.Values.AsEnumerable();

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        return Task.FromResult(query.ToList());
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _users.TryAdd(user.Id, user);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _users[user.Id] = user;
        return Task.CompletedTask;
    }

    public Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        var exists = _users.Values.Any(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }

    public Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default)
    {
        var exists = _users.Values.Any(u => u.Email.Equals(email));
        return Task.FromResult(exists);
    }
}
