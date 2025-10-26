using MediatR;
using Vendo.IdentityManagement.Application.Common.Models;
using Vendo.IdentityManagement.Application.DTOs;

namespace Vendo.IdentityManagement.Application.Queries.GetUsers;

/// <summary>
/// Query to get all users with optional filtering
/// </summary>
public class GetUsersQuery : IRequest<Result<List<UserDto>>>
{
    public bool? IsActive { get; set; }
}
