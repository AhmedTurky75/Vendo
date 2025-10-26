using MediatR;
using Vendo.CatalogManagement.Application.Common;

namespace Vendo.CatalogManagement.Application.Products.Commands.DeleteProduct;

/// <summary>
/// Command to delete a product.
/// </summary>
public class DeleteProductCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
}
