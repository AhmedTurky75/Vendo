using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vendo.IdentityManagement.Api.Models;
using Vendo.IdentityManagement.Application.Commands.ActivateUser;
using Vendo.IdentityManagement.Application.Commands.DeactivateUser;
using Vendo.IdentityManagement.Application.Queries.GetUser;
using Vendo.IdentityManagement.Application.Queries.GetUsers;

namespace Vendo.IdentityManagement.Api.Controllers;

/// <summary>
/// Administrative operations for managing users
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMediator mediator, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    /// <param name="isActive">Optional filter by active status</param>
    /// <returns>List of users</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<Application.DTOs.UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null)
    {
        var query = new GetUsersQuery { IsActive = isActive };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Application.DTOs.UserDto>>.ErrorResponse(result.Error ?? "Failed to retrieve users"));
        }

        return Ok(ApiResponse<List<Application.DTOs.UserDto>>.SuccessResponse(result.Data!));
    }

    /// <summary>
    /// Get a specific user by ID (Admin only)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User information</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Application.DTOs.UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<Application.DTOs.UserDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetUserQuery { UserId = id };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<Application.DTOs.UserDto>.ErrorResponse(result.Error ?? "User not found"));
        }

        return Ok(ApiResponse<Application.DTOs.UserDto>.SuccessResponse(result.Data!));
    }

    /// <summary>
    /// Activate a user account (Admin only)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success or error response</returns>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var command = new ActivateUserCommand { UserId = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to activate user {UserId}: {Error}", id, result.Error);
            return NotFound(ApiResponse<object>.ErrorResponse(result.Error ?? "Activation failed"));
        }

        _logger.LogInformation("User activated successfully: {UserId}", id);
        return Ok(ApiResponse<object>.SuccessResponse(new { message = "User activated successfully" }));
    }

    /// <summary>
    /// Deactivate a user account (Admin only)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success or error response</returns>
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var command = new DeactivateUserCommand { UserId = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to deactivate user {UserId}: {Error}", id, result.Error);
            return NotFound(ApiResponse<object>.ErrorResponse(result.Error ?? "Deactivation failed"));
        }

        _logger.LogInformation("User deactivated successfully: {UserId}", id);
        return Ok(ApiResponse<object>.SuccessResponse(new { message = "User deactivated successfully" }));
    }
}
