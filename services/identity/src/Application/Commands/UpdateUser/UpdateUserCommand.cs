using MediatR;
using Vendo.IdentityManagement.Application.Common.Models;
using Vendo.IdentityManagement.Application.DTOs;

namespace Vendo.IdentityManagement.Application.Commands.UpdateUser;

/// <summary>
/// Command to update user profile
/// </summary>
public class UpdateUserCommand : IRequest<Result<UserDto>>
{
    public Guid UserId { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Guid? StoreId { get; set; }
    public string? Address { get; set; }
    public string? Preferences { get; set; }
}
