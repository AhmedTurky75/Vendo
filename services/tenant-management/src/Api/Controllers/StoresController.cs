using MediatR;
using Microsoft.AspNetCore.Mvc;
using Vendo.TenantManagement.Application.Stores.Commands.CreateStore;
using Vendo.TenantManagement.Application.Stores.Commands.UpdateStore;
using Vendo.TenantManagement.Application.Stores.Commands.DeleteStore;
using Vendo.TenantManagement.Application.Stores.Queries.GetStore;
using Vendo.TenantManagement.Application.Stores.Queries.GetStoresByMerchant;

namespace Vendo.TenantManagement.Api.Controllers;

/// <summary>
/// API controller for managing stores/tenants.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StoresController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StoresController> _logger;

    public StoresController(IMediator mediator, ILogger<StoresController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new store.
    /// </summary>
    /// <param name="command">The store creation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created store.</returns>
    /// <response code="201">Store created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="409">Subdomain already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Application.Stores.DTOs.StoreDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateStore(
        [FromBody] CreateStoreCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating store with subdomain: {Subdomain}", command.Subdomain);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("already taken", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Conflict(new { error = result.Error, errors = result.Errors });
            }
            return BadRequest(new { error = result.Error, errors = result.Errors });
        }

        return CreatedAtAction(
            nameof(GetStore),
            new { id = result.Value!.Id },
            result.Value);
    }

    /// <summary>
    /// Gets a store by ID.
    /// </summary>
    /// <param name="id">The store ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The store details.</returns>
    /// <response code="200">Store found.</response>
    /// <response code="404">Store not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Application.Stores.DTOs.StoreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStore(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting store with ID: {StoreId}", id);

        var query = new GetStoreQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a store by subdomain.
    /// </summary>
    /// <param name="subdomain">The store subdomain.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The store details.</returns>
    /// <response code="200">Store found.</response>
    /// <response code="404">Store not found.</response>
    [HttpGet("by-subdomain/{subdomain}")]
    [ProducesResponseType(typeof(Application.Stores.DTOs.StoreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStoreBySubdomain(
        [FromRoute] string subdomain,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting store with subdomain: {Subdomain}", subdomain);

        var query = new GetStoreQuery { Subdomain = subdomain };
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets all stores owned by a specific merchant.
    /// </summary>
    /// <param name="ownerId">The owner user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of stores owned by the merchant.</returns>
    /// <response code="200">Stores retrieved successfully.</response>
    /// <response code="400">Invalid owner ID.</response>
    [HttpGet("by-owner/{ownerId}")]
    [ProducesResponseType(typeof(List<Application.Stores.DTOs.StoreDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStoresByMerchant(
        [FromRoute] string ownerId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting stores for owner: {OwnerId}", ownerId);

        var query = new GetStoresByMerchantQuery { OwnerId = ownerId };
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Updates an existing store.
    /// </summary>
    /// <param name="id">The store ID.</param>
    /// <param name="command">The updated store details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated store.</returns>
    /// <response code="200">Store updated successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="404">Store not found.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Application.Stores.DTOs.StoreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStore(
        [FromRoute] Guid id,
        [FromBody] UpdateStoreCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating store with ID: {StoreId}", id);

        command.Id = id;
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
            {
                return NotFound(new { error = result.Error });
            }
            return BadRequest(new { error = result.Error, errors = result.Errors });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes a store.
    /// </summary>
    /// <param name="id">The store ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">Store deleted successfully.</response>
    /// <response code="404">Store not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStore(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting store with ID: {StoreId}", id);

        var command = new DeleteStoreCommand { Id = id };
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return NoContent();
    }
}
