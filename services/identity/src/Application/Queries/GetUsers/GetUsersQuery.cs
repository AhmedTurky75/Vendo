using MediatR;
using Vendo.Identity.Application.Common.Models;
using Vendo.Identity.Application.DTOs;

namespace Vendo.Identity.Application.Queries.GetUsers;

/// <summary>
/// Query to get all users with optional filtering
/// </summary>
public class GetUsersQuery : IRequest<Result<List<UserDto>>>
{
    public bool? IsActive { get; set; }
}
