using Vendo.PaymentManagement.Domain.Entities;

namespace Vendo.PaymentManagement.Domain.Repositories;

/// <summary>
/// Repository interface for Payment aggregate.
/// </summary>
public interface IPaymentRepository
{
    /// <summary>
    /// Gets a payment by its unique identifier.
    /// </summary>
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all payments for a specific order.
    /// </summary>
    Task<List<Payment>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all payments for a specific customer.
    /// </summary>
    Task<List<Payment>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all payments for a specific tenant.
    /// </summary>
    Task<List<Payment>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all payments with pagination.
    /// </summary>
    Task<List<Payment>> GetAllAsync(int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new payment to the repository.
    /// </summary>
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing payment.
    /// </summary>
    void Update(Payment payment);

    /// <summary>
    /// Deletes a payment.
    /// </summary>
    void Delete(Payment payment);

    /// <summary>
    /// Saves all changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
