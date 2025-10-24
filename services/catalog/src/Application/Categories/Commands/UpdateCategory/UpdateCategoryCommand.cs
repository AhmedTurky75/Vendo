using MediatR;
using Vendo.Catalog.Application.Categories.DTOs;
using Vendo.Catalog.Application.Common;

namespace Vendo.Catalog.Application.Categories.Commands.UpdateCategory;

/// <summary>
/// Command to update an existing category.
/// </summary>
public class UpdateCategoryCommand : IRequest<Result<CategoryDto>>
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
}
