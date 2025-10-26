using MediatR;
using Vendo.CatalogManagement.Application.Common;

namespace Vendo.CatalogManagement.Application.Categories.Commands.DeleteCategory;

/// <summary>
/// Command to delete a category.
/// </summary>
public class DeleteCategoryCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
}
