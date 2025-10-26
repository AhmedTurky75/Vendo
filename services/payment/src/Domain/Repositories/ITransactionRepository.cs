using Vendo.PaymentManagement.Domain.Entities;

namespace Vendo.PaymentManagement.Domain.Repositories;

/// <summary>
/// Repository interface for Transaction aggregate.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Gets all transactions for a specific payment.
    /// </summary>
    Task<List<Transaction>> GetByPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all transactions with pagination.
    /// </summary>
    Task<List<Transaction>> GetAllAsync(int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new transaction to the repository.
    /// </summary>
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
