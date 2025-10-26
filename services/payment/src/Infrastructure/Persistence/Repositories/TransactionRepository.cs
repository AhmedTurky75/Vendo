using Microsoft.EntityFrameworkCore;
using Vendo.PaymentManagement.Domain.Entities;
using Vendo.PaymentManagement.Domain.Repositories;

namespace Vendo.PaymentManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Transaction entity.
/// </summary>
public class TransactionRepository : ITransactionRepository
{
    private readonly PaymentDbContext _context;

    public TransactionRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<List<Transaction>> GetByPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Where(t => t.PaymentId == paymentId)
            .OrderByDescending(t => t.ProcessedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Transaction>> GetAllAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .OrderByDescending(t => t.ProcessedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
