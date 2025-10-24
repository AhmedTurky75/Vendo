using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.TenantManagement.Application.Common;
using Vendo.TenantManagement.Domain.Interfaces;

namespace Vendo.TenantManagement.Application.Stores.Commands.DeleteStore;

/// <summary>
/// Handler for deleting a store.
/// </summary>
public class DeleteStoreCommandHandler : IRequestHandler<DeleteStoreCommand, Result<bool>>
{
    private readonly IStoreRepository _storeRepository;
    private readonly ILogger<DeleteStoreCommandHandler> _logger;

    public DeleteStoreCommandHandler(
        IStoreRepository storeRepository,
        ILogger<DeleteStoreCommandHandler> logger)
    {
        _storeRepository = storeRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteStoreCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting store with ID: {StoreId}", request.Id);

        // Check if store exists
        var store = await _storeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (store == null)
        {
            _logger.LogWarning("Store not found with ID: {StoreId}", request.Id);
            return Result<bool>.Failure("Store not found");
        }

        // Delete the store
        _storeRepository.Delete(store);
        await _storeRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Store deleted successfully with ID: {StoreId}", request.Id);
        return Result<bool>.Success(true);
    }
}
