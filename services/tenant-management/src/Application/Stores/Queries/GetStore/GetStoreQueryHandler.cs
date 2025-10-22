using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.TenantManagement.Application.Common.Models;
using Vendo.TenantManagement.Application.Stores.DTOs;
using Vendo.TenantManagement.Domain.Entities;
using Vendo.TenantManagement.Domain.Interfaces;

namespace Vendo.TenantManagement.Application.Stores.Queries.GetStore;

/// <summary>
/// Handler for GetStoreQuery.
/// </summary>
public class GetStoreQueryHandler : IRequestHandler<GetStoreQuery, Result<StoreDto>>
{
    private readonly IStoreRepository _storeRepository;
    private readonly ILogger<GetStoreQueryHandler> _logger;

    public GetStoreQueryHandler(
        IStoreRepository storeRepository,
        ILogger<GetStoreQueryHandler> logger)
    {
        _storeRepository = storeRepository;
        _logger = logger;
    }

    public async Task<Result<StoreDto>> Handle(GetStoreQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Store? store = null;

            if (request.Id.HasValue)
            {
                _logger.LogInformation("Getting store by ID: {StoreId}", request.Id.Value);
                store = await _storeRepository.GetByIdAsync(request.Id.Value, cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(request.Subdomain))
            {
                _logger.LogInformation("Getting store by subdomain: {Subdomain}", request.Subdomain);
                store = await _storeRepository.GetBySubdomainAsync(request.Subdomain, cancellationToken);
            }
            else
            {
                _logger.LogWarning("GetStoreQuery requires either Id or Subdomain");
                return Result<StoreDto>.Failure("Either store ID or subdomain must be provided");
            }

            if (store == null)
            {
                _logger.LogWarning("Store not found");
                return Result<StoreDto>.Failure("Store not found");
            }

            var storeDto = MapToDto(store);
            return Result<StoreDto>.Success(storeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting store");
            return Result<StoreDto>.Failure("An error occurred while retrieving the store");
        }
    }

    private static StoreDto MapToDto(Store store)
    {
        return new StoreDto
        {
            Id = store.Id,
            Name = store.Name,
            Subdomain = store.Subdomain.Value,
            Status = store.Status.ToString(),
            SubscriptionTier = store.SubscriptionTier.ToString(),
            OwnerId = store.OwnerId,
            MerchantEmail = store.MerchantInfo.Email,
            MerchantPhone = store.MerchantInfo.Phone,
            MerchantBusinessName = store.MerchantInfo.BusinessName,
            Currency = store.Settings.Currency,
            Timezone = store.Settings.Timezone,
            IsInTrial = store.IsInTrial,
            TrialEndsAt = store.TrialEndsAt,
            CreatedAt = store.CreatedAt,
            UpdatedAt = store.UpdatedAt
        };
    }
}
