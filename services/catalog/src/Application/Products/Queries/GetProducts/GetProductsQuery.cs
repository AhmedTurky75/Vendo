using MediatR;
using Vendo.CatalogManagement.Application.Common;
using Vendo.CatalogManagement.Application.Products.DTOs;

namespace Vendo.CatalogManagement.Application.Products.Queries.GetProducts;

/// <summary>
/// Query to get all products with pagination.
/// </summary>
public class GetProductsQuery : IRequest<Result<PagedResult<ProductDto>>>
{
    public Guid TenantId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
