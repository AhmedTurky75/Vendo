using AutoMapper;
using MediatR;
using Vendo.Identity.Application.Common.Models;
using Vendo.Identity.Application.DTOs;
using Vendo.Identity.Domain.Repositories;

namespace Vendo.Identity.Application.Queries.GetUsers;

/// <summary>
/// Handler for GetUsersQuery
/// </summary>
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<List<UserDto>>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUsersQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var users = await _userRepository.GetAllAsync(request.IsActive, cancellationToken);
            var userDtos = _mapper.Map<List<UserDto>>(users);
            return Result<List<UserDto>>.Success(userDtos);
        }
        catch (Exception ex)
        {
            return Result<List<UserDto>>.Failure($"An error occurred while retrieving users: {ex.Message}");
        }
    }
}
