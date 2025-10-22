using Vendo.TenantManagement.Domain.Entities;
using Vendo.TenantManagement.Domain.ValueObjects;

namespace Vendo.TenantManagement.Domain.Interfaces;

/// <summary>
/// Repository interface for Store aggregate operations.
/// </summary>
public interface IStoreRepository
{
    /// <summary>
    /// Gets a store by its unique identifier.
    /// </summary>
    /// <param name="id">The store ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The store if found, otherwise null.</returns>
    Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a store by its subdomain.
    /// </summary>
    /// <param name="subdomain">The subdomain.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The store if found, otherwise null.</returns>
    Task<Store?> GetBySubdomainAsync(Subdomain subdomain, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a store by its subdomain string.
    /// </summary>
    /// <param name="subdomainString">The subdomain as string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The store if found, otherwise null.</returns>
    Task<Store?> GetBySubdomainAsync(string subdomainString, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all stores owned by a specific merchant.
    /// </summary>
    /// <param name="ownerId">The owner user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of stores owned by the merchant.</returns>
    Task<IReadOnlyList<Store>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all stores with pagination.
    /// </summary>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to take.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of stores.</returns>
    Task<IReadOnlyList<Store>> GetAllAsync(int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a subdomain is already in use.
    /// </summary>
    /// <param name="subdomain">The subdomain to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if subdomain exists, otherwise false.</returns>
    Task<bool> SubdomainExistsAsync(string subdomain, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new store to the repository.
    /// </summary>
    /// <param name="store">The store to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(Store store, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing store.
    /// </summary>
    /// <param name="store">The store to update.</param>
    void Update(Store store);

    /// <summary>
    /// Deletes a store.
    /// </summary>
    /// <param name="store">The store to delete.</param>
    void Delete(Store store);

    /// <summary>
    /// Saves all pending changes.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of affected records.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
