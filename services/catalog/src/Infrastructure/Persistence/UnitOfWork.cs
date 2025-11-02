using Microsoft.EntityFrameworkCore.Storage;
using Vendo.CatalogManagement.Domain.Interfaces;
using Vendo.CatalogManagement.Infrastructure.Repositories;

namespace Vendo.CatalogManagement.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation for coordinating repositories and transactions.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly CatalogDbContext _context;
    private IDbContextTransaction? _currentTransaction;
    private readonly Lazy<IProductRepository> _productRepository;
    private readonly Lazy<ICategoryRepository> _categoryRepository;

    public UnitOfWork(CatalogDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));

        _productRepository = new Lazy<IProductRepository>(() => new ProductRepository(_context));
        _categoryRepository = new Lazy<ICategoryRepository>(() => new CategoryRepository(_context));
    }

    public IProductRepository Products => _productRepository.Value;
    public ICategoryRepository Categories => _categoryRepository.Value;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before saving
        await DispatchDomainEventsAsync(cancellationToken);

        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
            throw new InvalidOperationException("A transaction is already in progress");

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
            throw new InvalidOperationException("No transaction in progress");

        try
        {
            await SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
            throw new InvalidOperationException("No transaction in progress");

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var domainEntities = _context.ChangeTracker
            .Entries<Domain.Common.BaseEntity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        // Clear events from entities
        domainEntities.ForEach(entity => entity.ClearDomainEvents());

        // Dispatch events via MediatR (would need to inject IMediator for this)
        // For now, events are just cleared
        // In a full implementation, you would inject IPublisher and publish each event
        // foreach (var domainEvent in domainEvents)
        // {
        //     await _publisher.Publish(domainEvent, cancellationToken);
        // }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context?.Dispose();
    }
}
