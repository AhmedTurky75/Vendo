using MediatR;
using Vendo.Catalog.Application.Categories.DTOs;
using Vendo.Catalog.Application.Common;

namespace Vendo.Catalog.Application.Categories.Queries.GetCategory;

/// <summary>
/// Query to get a category by ID.
/// </summary>
public class GetCategoryQuery : IRequest<Result<CategoryDto>>
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
}
