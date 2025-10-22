using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.TenantManagement.Application.Common.Models;
using Vendo.TenantManagement.Application.Stores.DTOs;
using Vendo.TenantManagement.Domain.Entities;
using Vendo.TenantManagement.Domain.Interfaces;

namespace Vendo.TenantManagement.Application.Stores.Queries.GetStoresByMerchant;

/// <summary>
/// Handler for GetStoresByMerchantQuery.
/// </summary>
public class GetStoresByMerchantQueryHandler : IRequestHandler<GetStoresByMerchantQuery, Result<List<StoreDto>>>
{
    private readonly IStoreRepository _storeRepository;
    private readonly ILogger<GetStoresByMerchantQueryHandler> _logger;

    public GetStoresByMerchantQueryHandler(
        IStoreRepository storeRepository,
        ILogger<GetStoresByMerchantQueryHandler> logger)
    {
        _storeRepository = storeRepository;
        _logger = logger;
    }

    public async Task<Result<List<StoreDto>>> Handle(GetStoresByMerchantQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.OwnerId))
            {
                _logger.LogWarning("Owner ID is required");
                return Result<List<StoreDto>>.Failure("Owner ID is required");
            }

            _logger.LogInformation("Getting stores for owner: {OwnerId}", request.OwnerId);

            var stores = await _storeRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);

            var storeDtos = stores.Select(MapToDto).ToList();

            _logger.LogInformation("Found {Count} stores for owner: {OwnerId}", storeDtos.Count, request.OwnerId);

            return Result<List<StoreDto>>.Success(storeDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stores for owner: {OwnerId}", request.OwnerId);
            return Result<List<StoreDto>>.Failure("An error occurred while retrieving stores");
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
