using MediatR;
using Vendo.CatalogManagement.Application.Common;
using Vendo.CatalogManagement.Application.Products.DTOs;

namespace Vendo.CatalogManagement.Application.Products.Queries.GetProduct;

/// <summary>
/// Query to get a product by ID.
/// </summary>
public class GetProductQuery : IRequest<Result<ProductDto>>
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
}
