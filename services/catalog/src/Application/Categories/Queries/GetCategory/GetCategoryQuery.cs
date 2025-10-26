using MediatR;
using Vendo.CatalogManagement.Application.Categories.DTOs;
using Vendo.CatalogManagement.Application.Common;

namespace Vendo.CatalogManagement.Application.Categories.Queries.GetCategory;

/// <summary>
/// Query to get a category by ID.
/// </summary>
public class GetCategoryQuery : IRequest<Result<CategoryDto>>
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
}
