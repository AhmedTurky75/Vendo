using MediatR;
using Vendo.Catalog.Application.Common;

namespace Vendo.Catalog.Application.Products.Commands.DeleteProduct;

/// <summary>
/// Command to delete a product.
/// </summary>
public class DeleteProductCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
}
