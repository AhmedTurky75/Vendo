using Vendo.Identity.Domain.Entities;
using Vendo.Identity.Domain.ValueObjects;

namespace Vendo.Identity.Domain.Repositories;

/// <summary>
/// Repository interface for User aggregate
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by their unique identifier
    /// </summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their username
    /// </summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their email address
    /// </summary>
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users with optional filtering
    /// </summary>
    Task<List<User>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user to the repository
    /// </summary>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user
    /// </summary>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a username already exists
    /// </summary>
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an email already exists
    /// </summary>
    Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default);
}
