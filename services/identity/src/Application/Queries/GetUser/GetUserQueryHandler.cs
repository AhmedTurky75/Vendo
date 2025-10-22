using AutoMapper;
using MediatR;
using Vendo.Identity.Application.Common.Models;
using Vendo.Identity.Application.DTOs;
using Vendo.Identity.Domain.Repositories;

namespace Vendo.Identity.Application.Queries.GetUser;

/// <summary>
/// Handler for GetUserQuery
/// </summary>
public class GetUserQueryHandler : IRequestHandler<GetUserQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUserQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            var userDto = _mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"An error occurred while retrieving user: {ex.Message}");
        }
    }
}
