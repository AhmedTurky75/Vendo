using AutoMapper;
using Vendo.IdentityManagement.Application.DTOs;
using Vendo.IdentityManagement.Domain.Entities;

namespace Vendo.IdentityManagement.Application.Mappings;

/// <summary>
/// AutoMapper profile for User entity mappings
/// </summary>
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Roles, opt => opt.Ignore()); // Roles are populated separately in handlers
    }
}
