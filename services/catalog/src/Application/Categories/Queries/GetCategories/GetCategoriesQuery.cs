using MediatR;
using Vendo.Catalog.Application.Categories.DTOs;
using Vendo.Catalog.Application.Common;

namespace Vendo.Catalog.Application.Categories.Queries.GetCategories;

/// <summary>
/// Query to get all categories for a tenant.
/// </summary>
public class GetCategoriesQuery : IRequest<Result<List<CategoryDto>>>
{
    public Guid TenantId { get; set; }
    public bool? OnlyRoot { get; set; } = false;
}
