using AutoMapper;
using SkyReserve.Application.DTOs.Price.DTOs;
using DomainPrice = SkyReserve.Domain.Entities.Price;

namespace SkyReserve.Application.Mapping
{
    public class PriceMappingProfile : Profile
    {
        public PriceMappingProfile()
        {
            CreateMap<DomainPrice, PriceDto>();
        }
    }
}
