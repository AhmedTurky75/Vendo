using MediatR;
using Vendo.Identity.Application.Common.Models;
using Vendo.Identity.Application.DTOs;

namespace Vendo.Identity.Application.Queries.GetUser;

/// <summary>
/// Query to get a user by ID
/// </summary>
public class GetUserQuery : IRequest<Result<UserDto>>
{
    public Guid UserId { get; set; }
}
