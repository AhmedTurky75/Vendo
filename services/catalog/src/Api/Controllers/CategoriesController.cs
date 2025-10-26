using MediatR;
using Microsoft.AspNetCore.Mvc;
using Vendo.CatalogManagement.Application.Categories.Commands.CreateCategory;
using Vendo.CatalogManagement.Application.Categories.Commands.DeleteCategory;
using Vendo.CatalogManagement.Application.Categories.Commands.UpdateCategory;
using Vendo.CatalogManagement.Application.Categories.Queries.GetCategories;
using Vendo.CatalogManagement.Application.Categories.Queries.GetCategory;
using Vendo.CatalogManagement.Application.Categories.Queries.GetCategoryWithProducts;

namespace Vendo.CatalogManagement.Api.Controllers;

/// <summary>
/// Categories management endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(IMediator mediator, ILogger<CategoriesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="command">Category creation details</param>
    /// <returns>The created category</returns>
    /// <response code="200">Category created successfully</response>
    /// <response code="400">Invalid request or validation error</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error, errors = result.Errors });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a category by ID.
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>The category details</returns>
    /// <response code="200">Category found</response>
    /// <response code="404">Category not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory(Guid id, [FromHeader(Name = "X-Tenant-Id")] Guid tenantId)
    {
        var query = new GetCategoryQuery { Id = id, TenantId = tenantId };
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets all categories for a tenant.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="onlyRoot">If true, returns only root categories (no parent)</param>
    /// <returns>List of categories</returns>
    /// <response code="200">Categories retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromQuery] bool onlyRoot = false)
    {
        var query = new GetCategoriesQuery
        {
            TenantId = tenantId,
            OnlyRoot = onlyRoot
        };

        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a category with its products.
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>The category with products</returns>
    /// <response code="200">Category with products found</response>
    /// <response code="404">Category not found</response>
    [HttpGet("{id}/products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryWithProducts(Guid id, [FromHeader(Name = "X-Tenant-Id")] Guid tenantId)
    {
        var query = new GetCategoryWithProductsQuery { Id = id, TenantId = tenantId };
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="command">Category update details</param>
    /// <returns>The updated category</returns>
    /// <response code="200">Category updated successfully</response>
    /// <response code="400">Invalid request or validation error</response>
    /// <response code="404">Category not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(new { error = result.Error });
            }
            return BadRequest(new { error = result.Error, errors = result.Errors });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes a category.
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Success indicator</returns>
    /// <response code="200">Category deleted successfully</response>
    /// <response code="400">Category has dependencies and cannot be deleted</response>
    /// <response code="404">Category not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(Guid id, [FromHeader(Name = "X-Tenant-Id")] Guid tenantId)
    {
        var command = new DeleteCategoryCommand { Id = id, TenantId = tenantId };
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(new { error = result.Error });
            }
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { success = true, message = "Category deleted successfully" });
    }
}
