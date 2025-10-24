using MediatR;
using Vendo.Catalog.Application.Common;
using Vendo.Catalog.Application.Products.DTOs;

namespace Vendo.Catalog.Application.Products.Queries.GetProductsByCategory;

/// <summary>
/// Query to get products by category with pagination.
/// </summary>
public class GetProductsByCategoryQuery : IRequest<Result<PagedResult<ProductDto>>>
{
    public Guid CategoryId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
