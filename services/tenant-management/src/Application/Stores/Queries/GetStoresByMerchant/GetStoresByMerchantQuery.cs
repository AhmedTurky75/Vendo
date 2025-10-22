using MediatR;
using Vendo.TenantManagement.Application.Common.Models;
using Vendo.TenantManagement.Application.Stores.DTOs;

namespace Vendo.TenantManagement.Application.Stores.Queries.GetStoresByMerchant;

/// <summary>
/// Query to get all stores owned by a specific merchant.
/// </summary>
public sealed class GetStoresByMerchantQuery : IRequest<Result<List<StoreDto>>>
{
    /// <summary>
    /// Gets or sets the owner user ID.
    /// </summary>
    public string OwnerId { get; set; } = string.Empty;
}
