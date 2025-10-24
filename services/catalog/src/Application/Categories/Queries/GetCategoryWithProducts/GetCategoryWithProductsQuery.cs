using MediatR;
using Vendo.Catalog.Application.Categories.DTOs;
using Vendo.Catalog.Application.Common;

namespace Vendo.Catalog.Application.Categories.Queries.GetCategoryWithProducts;

/// <summary>
/// Query to get a category with its products.
/// </summary>
public class GetCategoryWithProductsQuery : IRequest<Result<CategoryWithProductsDto>>
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
}
