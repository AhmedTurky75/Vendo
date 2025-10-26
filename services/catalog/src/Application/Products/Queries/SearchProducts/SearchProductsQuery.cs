using MediatR;
using Vendo.CatalogManagement.Application.Common;
using Vendo.CatalogManagement.Application.Products.DTOs;

namespace Vendo.CatalogManagement.Application.Products.Queries.SearchProducts;

/// <summary>
/// Query to search products by name or description.
/// </summary>
public class SearchProductsQuery : IRequest<Result<PagedResult<ProductDto>>>
{
    public Guid TenantId { get; set; }
    public string SearchTerm { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
