using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.TenantManagement.Application.Common.Models;
using Vendo.TenantManagement.Application.Stores.DTOs;
using Vendo.TenantManagement.Domain.Interfaces;
using Vendo.TenantManagement.Domain.ValueObjects;

namespace Vendo.TenantManagement.Application.Stores.Commands.UpdateStore;

/// <summary>
/// Handler for UpdateStoreCommand.
/// </summary>
public class UpdateStoreCommandHandler : IRequestHandler<UpdateStoreCommand, Result<StoreDto>>
{
    private readonly IStoreRepository _storeRepository;
    private readonly ILogger<UpdateStoreCommandHandler> _logger;

    public UpdateStoreCommandHandler(
        IStoreRepository storeRepository,
        ILogger<UpdateStoreCommandHandler> logger)
    {
        _storeRepository = storeRepository;
        _logger = logger;
    }

    public async Task<Result<StoreDto>> Handle(UpdateStoreCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating store with ID: {StoreId}", request.Id);

            // Get existing store
            var store = await _storeRepository.GetByIdAsync(request.Id, cancellationToken);
            if (store == null)
            {
                _logger.LogWarning("Store not found with ID: {StoreId}", request.Id);
                return Result<StoreDto>.Failure("Store not found");
            }

            // Update store name
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                store.UpdateName(request.Name);
            }

            // Update merchant info
            var merchantInfo = MerchantInfo.Create(
                email: request.MerchantEmail,
                phone: request.MerchantPhone,
                businessName: request.MerchantBusinessName,
                address: request.MerchantAddress,
                city: request.MerchantCity,
                state: request.MerchantState,
                postalCode: request.MerchantPostalCode,
                country: request.MerchantCountry
            );
            store.UpdateMerchantInfo(merchantInfo);

            // Update settings if provided
            if (request.Currency != null || request.Timezone != null || request.Language != null ||
                request.TaxRate.HasValue || request.TaxEnabled.HasValue ||
                request.PrimaryColor != null || request.AccentColor != null || request.LogoUrl != null)
            {
                var settings = StoreSettings.Create(
                    currency: request.Currency ?? store.Settings.Currency,
                    timezone: request.Timezone ?? store.Settings.Timezone,
                    language: request.Language ?? store.Settings.Language,
                    taxRate: request.TaxRate ?? store.Settings.TaxRate,
                    taxEnabled: request.TaxEnabled ?? store.Settings.TaxEnabled,
                    primaryColor: request.PrimaryColor ?? store.Settings.PrimaryColor,
                    accentColor: request.AccentColor ?? store.Settings.AccentColor,
                    logoUrl: request.LogoUrl ?? store.Settings.LogoUrl
                );
                store.UpdateSettings(settings);
            }

            // Save changes
            _storeRepository.Update(store);
            await _storeRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Store updated successfully with ID: {StoreId}", store.Id);

            // Map to DTO
            var storeDto = MapToDto(store);

            return Result<StoreDto>.Success(storeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating store with ID: {StoreId}", request.Id);
            return Result<StoreDto>.Failure("An error occurred while updating the store");
        }
    }

    private static StoreDto MapToDto(Domain.Entities.Store store)
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
