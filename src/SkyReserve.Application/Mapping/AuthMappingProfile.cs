using AutoMapper;
using SkyReserve.Application.Contract.Authentication;
using SkyReserve.Domain.Entities;

namespace SkyReserve.Application.Mapping
{
    public class AuthMappingProfile : Profile
    {
        public AuthMappingProfile()
        {
            CreateMap<RegisterRequest, ApplicationUser>();

        }

    }
}
