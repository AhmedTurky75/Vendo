using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.TenantManagement.Application.Common.Models;
using Vendo.TenantManagement.Application.Stores.DTOs;
using Vendo.TenantManagement.Domain.Entities;
using Vendo.TenantManagement.Domain.Interfaces;
using Vendo.TenantManagement.Domain.ValueObjects;

namespace Vendo.TenantManagement.Application.Stores.Commands.CreateStore;

/// <summary>
/// Handler for CreateStoreCommand.
/// </summary>
public class CreateStoreCommandHandler : IRequestHandler<CreateStoreCommand, Result<StoreDto>>
{
    private readonly IStoreRepository _storeRepository;
    private readonly ILogger<CreateStoreCommandHandler> _logger;

    public CreateStoreCommandHandler(
        IStoreRepository storeRepository,
        ILogger<CreateStoreCommandHandler> logger)
    {
        _storeRepository = storeRepository;
        _logger = logger;
    }

    public async Task<Result<StoreDto>> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating store with subdomain: {Subdomain}", request.Subdomain);

            // Check if subdomain already exists
            var subdomainExists = await _storeRepository.SubdomainExistsAsync(
                request.Subdomain,
                cancellationToken);

            if (subdomainExists)
            {
                _logger.LogWarning("Subdomain {Subdomain} already exists", request.Subdomain);
                return Result<StoreDto>.Failure($"Subdomain '{request.Subdomain}' is already taken");
            }

            // Create subdomain value object
            var subdomainResult = Subdomain.Create(request.Subdomain);
            if (subdomainResult.IsFailure)
            {
                _logger.LogWarning("Invalid subdomain: {Error}", subdomainResult.Error);
                return Result<StoreDto>.Failure(subdomainResult.Error!);
            }

            // Create merchant info value object
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

            // Create store entity
            var store = Store.Create(
                name: request.Name,
                subdomain: subdomainResult.Value!,
                merchantInfo: merchantInfo,
                ownerId: request.OwnerId
            );

            // Save to repository
            await _storeRepository.AddAsync(store, cancellationToken);
            await _storeRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Store created successfully with ID: {StoreId}", store.Id);

            // Map to DTO
            var storeDto = MapToDto(store);

            return Result<StoreDto>.Success(storeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating store with subdomain: {Subdomain}", request.Subdomain);
            return Result<StoreDto>.Failure("An error occurred while creating the store");
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
