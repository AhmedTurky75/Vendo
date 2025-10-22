using MediatR;
using Vendo.TenantManagement.Application.Common.Models;
using Vendo.TenantManagement.Application.Stores.DTOs;

namespace Vendo.TenantManagement.Application.Stores.Queries.GetStore;

/// <summary>
/// Query to get a store by ID or subdomain.
/// </summary>
public sealed class GetStoreQuery : IRequest<Result<StoreDto>>
{
    /// <summary>
    /// Gets or sets the store ID (optional if subdomain is provided).
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Gets or sets the subdomain (optional if ID is provided).
    /// </summary>
    public string? Subdomain { get; set; }
}
