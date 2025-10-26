using MediatR;
using Vendo.CatalogManagement.Application.Categories.DTOs;
using Vendo.CatalogManagement.Application.Common;

namespace Vendo.CatalogManagement.Application.Categories.Queries.GetCategoryWithProducts;

/// <summary>
/// Query to get a category with its products.
/// </summary>
public class GetCategoryWithProductsQuery : IRequest<Result<CategoryWithProductsDto>>
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
}
