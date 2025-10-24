using MediatR;
using Microsoft.AspNetCore.Mvc;
using Vendo.Catalog.Application.Products.Commands.CreateProduct;
using Vendo.Catalog.Application.Products.Commands.DeleteProduct;
using Vendo.Catalog.Application.Products.Commands.UpdateProduct;
using Vendo.Catalog.Application.Products.Queries.GetProduct;
using Vendo.Catalog.Application.Products.Queries.GetProducts;
using Vendo.Catalog.Application.Products.Queries.GetProductsByCategory;
using Vendo.Catalog.Application.Products.Queries.SearchProducts;

namespace Vendo.Catalog.Api.Controllers;

/// <summary>
/// Products management endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="command">Product creation details</param>
    /// <returns>The created product</returns>
    /// <response code="200">Product created successfully</response>
    /// <response code="400">Invalid request or validation error</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error, errors = result.Errors });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>The product details</returns>
    /// <response code="200">Product found</response>
    /// <response code="404">Product not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(Guid id, [FromHeader(Name = "X-Tenant-Id")] Guid tenantId)
    {
        var query = new GetProductQuery { Id = id, TenantId = tenantId };
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets all products with pagination.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <returns>Paginated list of products</returns>
    /// <response code="200">Products retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageSize > 100) pageSize = 100;
        if (pageNumber < 1) pageNumber = 1;

        var query = new GetProductsQuery
        {
            TenantId = tenantId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets products by category.
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <returns>Paginated list of products in the category</returns>
    /// <response code="200">Products retrieved successfully</response>
    /// <response code="404">Category not found</response>
    [HttpGet("category/{categoryId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductsByCategory(
        Guid categoryId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageSize > 100) pageSize = 100;
        if (pageNumber < 1) pageNumber = 1;

        var query = new GetProductsByCategoryQuery
        {
            CategoryId = categoryId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Searches products by name, description, or SKU.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="q">Search term</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <returns>Paginated list of matching products</returns>
    /// <response code="200">Products retrieved successfully</response>
    /// <response code="400">Invalid search term</response>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchProducts(
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromQuery] string q,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageSize > 100) pageSize = 100;
        if (pageNumber < 1) pageNumber = 1;

        var query = new SearchProductsQuery
        {
            TenantId = tenantId,
            SearchTerm = q,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="command">Product update details</param>
    /// <returns>The updated product</returns>
    /// <response code="200">Product updated successfully</response>
    /// <response code="400">Invalid request or validation error</response>
    /// <response code="404">Product not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductCommand command)
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
    /// Deletes a product.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Success indicator</returns>
    /// <response code="200">Product deleted successfully</response>
    /// <response code="404">Product not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid id, [FromHeader(Name = "X-Tenant-Id")] Guid tenantId)
    {
        var command = new DeleteProductCommand { Id = id, TenantId = tenantId };
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(new { success = true, message = "Product deleted successfully" });
    }
}
