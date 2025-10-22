using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vendo.Identity.Api.Models;
using Vendo.Identity.Application.Commands.ChangePassword;
using Vendo.Identity.Application.Commands.ForgotPassword;
using Vendo.Identity.Application.Commands.RegisterUser;
using Vendo.Identity.Application.Commands.ResetPassword;
using Vendo.Identity.Application.Commands.UpdateUser;
using Vendo.Identity.Application.Queries.GetUser;

namespace Vendo.Identity.Api.Controllers;

/// <summary>
/// Handles user account operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AccountController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IMediator mediator, ILogger<AccountController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Created user information</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<Application.DTOs.UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<Application.DTOs.UserDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterUserCommand
        {
            Username = request.Username,
            Email = request.Email,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("User registration failed: {Error}", result.Error);
            return BadRequest(ApiResponse<Application.DTOs.UserDto>.ErrorResponse(
                result.Error ?? "Registration failed",
                result.ValidationErrors.Any() ? result.ValidationErrors : null));
        }

        _logger.LogInformation("User registered successfully: {Username}", request.Username);
        return CreatedAtAction(nameof(GetProfile), new { }, ApiResponse<Application.DTOs.UserDto>.SuccessResponse(result.Data!));
    }

    /// <summary>
    /// Get the current user's profile
    /// </summary>
    /// <returns>User profile information</returns>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<Application.DTOs.UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<Application.DTOs.UserDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var query = new GetUserQuery { UserId = userId.Value };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<Application.DTOs.UserDto>.ErrorResponse(result.Error ?? "User not found"));
        }

        return Ok(ApiResponse<Application.DTOs.UserDto>.SuccessResponse(result.Data!));
    }

    /// <summary>
    /// Update the current user's profile
    /// </summary>
    /// <param name="request">Updated profile information</param>
    /// <returns>Updated user information</returns>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<Application.DTOs.UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<Application.DTOs.UserDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var command = new UpdateUserCommand
        {
            UserId = userId.Value,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Profile update failed for user {UserId}: {Error}", userId, result.Error);
            return BadRequest(ApiResponse<Application.DTOs.UserDto>.ErrorResponse(
                result.Error ?? "Update failed",
                result.ValidationErrors.Any() ? result.ValidationErrors : null));
        }

        _logger.LogInformation("Profile updated successfully for user: {UserId}", userId);
        return Ok(ApiResponse<Application.DTOs.UserDto>.SuccessResponse(result.Data!));
    }

    /// <summary>
    /// Change the current user's password
    /// </summary>
    /// <param name="request">Password change details</param>
    /// <returns>Success or error response</returns>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var command = new ChangePasswordCommand
        {
            UserId = userId.Value,
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Password change failed for user {UserId}: {Error}", userId, result.Error);
            return BadRequest(ApiResponse<object>.ErrorResponse(
                result.Error ?? "Password change failed",
                result.ValidationErrors.Any() ? result.ValidationErrors : null));
        }

        _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
        return Ok(ApiResponse<object>.SuccessResponse(new { message = "Password changed successfully" }));
    }

    /// <summary>
    /// Request password reset (forgot password)
    /// </summary>
    /// <param name="request">Forgot password request with email</param>
    /// <returns>Success response</returns>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand
        {
            Email = request.Email
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Forgot password request failed: {Error}", result.Error);
            return BadRequest(ApiResponse<object>.ErrorResponse(
                result.Error ?? "Failed to process password reset request",
                result.ValidationErrors.Any() ? result.ValidationErrors : null));
        }

        _logger.LogInformation("Password reset email process initiated");
        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            message = "If the email exists, a password reset link has been sent. Please check your email."
        }));
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    /// <param name="request">Reset password request with token and new password</param>
    /// <returns>Success or error response</returns>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand
        {
            Email = request.Email,
            Token = request.Token,
            NewPassword = request.NewPassword
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Password reset failed: {Error}", result.Error);
            return BadRequest(ApiResponse<object>.ErrorResponse(
                result.Error ?? "Failed to reset password",
                result.ValidationErrors.Any() ? result.ValidationErrors : null));
        }

        _logger.LogInformation("Password reset successfully");
        return Ok(ApiResponse<object>.SuccessResponse(new { message = "Password has been reset successfully" }));
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }
        return userId;
    }
}
