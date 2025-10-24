using MediatR;
using Vendo.Catalog.Application.Common;
using Vendo.Catalog.Application.Products.DTOs;

namespace Vendo.Catalog.Application.Products.Queries.GetProducts;

/// <summary>
/// Query to get all products with pagination.
/// </summary>
public class GetProductsQuery : IRequest<Result<PagedResult<ProductDto>>>
{
    public Guid TenantId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
