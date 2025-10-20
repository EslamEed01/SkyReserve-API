
using AutoMapper;
using Learnova.Business.DTOs.Contract.Users;
using SkyReserve.Application.Contract.Authentication;
using SkyReserve.Domain.Entities;

namespace SkyReserve.Application.Mapping
{
    public class UserMappingProfile : Profile
    {

        public UserMappingProfile()
        {
            CreateMap<ApplicationUser, UserResponse>();
            CreateMap<ApplicationUser, UserProfileResponse>();

            CreateMap<UserWithRoles, UserResponse>()
           .ConstructUsing(src => new UserResponse(
                   src.User.Id,
                   src.User.FirstName,
                   src.User.LastName,
                   src.User.Email!,
                   src.User.RoleType,
                   src.User.IsDisabled,
                   src.Roles.ToList()));

            CreateMap<UpdateUserRequest, ApplicationUser>();
            CreateMap<RegisterRequest, ApplicationUser>();


        }
    }
}
