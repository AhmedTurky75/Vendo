using MediatR;
using Vendo.Catalog.Application.Common;

namespace Vendo.Catalog.Application.Categories.Commands.DeleteCategory;

/// <summary>
/// Command to delete a category.
/// </summary>
public class DeleteCategoryCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
}
