using MediatR;
using Vendo.Catalog.Application.Common;
using Vendo.Catalog.Application.Products.DTOs;

namespace Vendo.Catalog.Application.Products.Queries.GetProduct;

/// <summary>
/// Query to get a product by ID.
/// </summary>
public class GetProductQuery : IRequest<Result<ProductDto>>
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
}
