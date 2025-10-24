using MediatR;
using Vendo.TenantManagement.Application.Common;

namespace Vendo.TenantManagement.Application.Stores.Commands.DeleteStore;

/// <summary>
/// Command to delete a store.
/// </summary>
public class DeleteStoreCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// Gets or sets the store ID to delete.
    /// </summary>
    public Guid Id { get; set; }
}
