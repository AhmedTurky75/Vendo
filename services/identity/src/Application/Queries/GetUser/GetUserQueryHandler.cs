using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Vendo.IdentityManagement.Application.Common.Models;
using Vendo.IdentityManagement.Application.DTOs;
using Vendo.IdentityManagement.Domain.Entities;

namespace Vendo.IdentityManagement.Application.Queries.GetUser;

/// <summary>
/// Handler for GetUserQuery
/// </summary>
public class GetUserQueryHandler : IRequestHandler<GetUserQuery, Result<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public GetUserQueryHandler(
        UserManager<ApplicationUser> userManager,
        IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            
            if (user == null)
            {
                return Result<UserDto>.Failure($"User with ID '{request.UserId}' not found");
            }

            var userDto = _mapper.Map<UserDto>(user);
            
            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);
            userDto.Roles = roles.ToList();

            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"An error occurred while retrieving user: {ex.Message}");
        }
    }
}
