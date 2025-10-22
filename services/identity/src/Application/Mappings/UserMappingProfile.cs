using AutoMapper;
using Vendo.Identity.Application.DTOs;
using Vendo.Identity.Domain.Entities;

namespace Vendo.Identity.Application.Mappings;

/// <summary>
/// AutoMapper profile for User entity mappings
/// </summary>
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles.ToList()));
    }
}
