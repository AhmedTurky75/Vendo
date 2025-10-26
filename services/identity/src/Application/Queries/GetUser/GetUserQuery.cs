using MediatR;
using Vendo.IdentityManagement.Application.Common.Models;
using Vendo.IdentityManagement.Application.DTOs;

namespace Vendo.IdentityManagement.Application.Queries.GetUser;

/// <summary>
/// Query to get a user by ID
/// </summary>
public class GetUserQuery : IRequest<Result<UserDto>>
{
    public Guid UserId { get; set; }
}
