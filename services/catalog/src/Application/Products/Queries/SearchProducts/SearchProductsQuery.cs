using MediatR;
using Vendo.Catalog.Application.Common;
using Vendo.Catalog.Application.Products.DTOs;

namespace Vendo.Catalog.Application.Products.Queries.SearchProducts;

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
