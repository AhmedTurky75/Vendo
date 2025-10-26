using MediatR;
using Vendo.CatalogManagement.Application.Categories.DTOs;
using Vendo.CatalogManagement.Application.Common;

namespace Vendo.CatalogManagement.Application.Categories.Queries.GetCategories;

/// <summary>
/// Query to get all categories for a tenant.
/// </summary>
public class GetCategoriesQuery : IRequest<Result<List<CategoryDto>>>
{
    public Guid TenantId { get; set; }
    public bool? OnlyRoot { get; set; } = false;
}
