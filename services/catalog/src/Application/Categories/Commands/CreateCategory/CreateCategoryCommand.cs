using MediatR;
using Vendo.Catalog.Application.Categories.DTOs;
using Vendo.Catalog.Application.Common;

namespace Vendo.Catalog.Application.Categories.Commands.CreateCategory;

/// <summary>
/// Command to create a new category.
/// </summary>
public class CreateCategoryCommand : IRequest<Result<CategoryDto>>
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ImageUrl { get; set; }
}
