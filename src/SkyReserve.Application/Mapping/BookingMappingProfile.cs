using AutoMapper;
using SkyReserve.Application.Booking.DTOS;
using DomainBooking = SkyReserve.Domain.Entities.Booking;


namespace SkyReserve.Application.Mapping
{
    public class BookingMappingProfile : Profile
    {
        public BookingMappingProfile()
        {
            CreateMap<CreateBookingDto, DomainBooking>();

            CreateMap<UpdateBookingDto, DomainBooking>()
                .ForMember(dest => dest.BookingId, opt => opt.Ignore())
                .ForMember(dest => dest.BookingRef, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.FlightId, opt => opt.Ignore())
                .ForMember(dest => dest.BookingDate, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            CreateMap<DomainBooking, BookingDto>()
                .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => src.Flight != null ? src.Flight.FlightNumber : null))
                .ForMember(dest => dest.DepartureTime, opt => opt.MapFrom(src => src.Flight != null ? (DateTime?)src.Flight.DepartureTime : null))
                .ForMember(dest => dest.ArrivalTime, opt => opt.MapFrom(src => src.Flight != null ? (DateTime?)src.Flight.ArrivalTime : null))
                .ForMember(dest => dest.Passengers, opt => opt.MapFrom(src => src.Passengers));
        }
    }
}