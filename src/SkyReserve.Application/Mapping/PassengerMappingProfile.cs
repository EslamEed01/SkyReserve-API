using AutoMapper;
using SkyReserve.Application.Passenger.DTOS;
using DomainBooking = SkyReserve.Domain.Entities.Booking;
using DomainPassenger = SkyReserve.Domain.Entities.Passenger;

namespace SkyReserve.Application.Mapping
{
    public class PassengerMappingProfile : Profile
    {
        public PassengerMappingProfile()
        {
            CreateMap<DomainPassenger, PassengerDto>()
                .ForMember(dest => dest.BookingStatus, opt => opt.MapFrom(src => src.Booking != null ? src.Booking.Status : null))
                .ForMember(dest => dest.BookingDate, opt => opt.MapFrom(src => src.Booking != null ? src.Booking.BookingDate : (DateTime?)null))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.IsGuestPassenger, opt => opt.MapFrom(src => src.IsGuestPassenger));

            CreateMap<CreatePassengerDto, DomainPassenger>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));

            CreateMap<UpdatePassengerDto, DomainPassenger>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));

            CreateMap<DomainBooking, GuestBookingDetailsDto>()
                .ForMember(dest => dest.BookingRef, opt => opt.MapFrom(src => src.BookingRef))
                .ForMember(dest => dest.BookingStatus, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.BookingDate, opt => opt.MapFrom(src => src.BookingDate))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => src.Flight.FlightNumber))
                .ForMember(dest => dest.DepartureTime, opt => opt.MapFrom(src => src.Flight.DepartureTime))
                .ForMember(dest => dest.ArrivalTime, opt => opt.MapFrom(src => src.Flight.ArrivalTime))
                .ForMember(dest => dest.DepartureAirport, opt => opt.MapFrom(src =>
                    $"{src.Flight.DepartureAirport.Code} - {src.Flight.DepartureAirport.Name}, {src.Flight.DepartureAirport.City}"))
                .ForMember(dest => dest.ArrivalAirport, opt => opt.MapFrom(src =>
                    $"{src.Flight.ArrivalAirport.Code} - {src.Flight.ArrivalAirport.Name}, {src.Flight.ArrivalAirport.City}"))
                .ForMember(dest => dest.FlightStatus, opt => opt.MapFrom(src => src.Flight.Status))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.Payment != null ? src.Payment.PaymentStatus : "Unknown"))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.Payment != null ? src.Payment.PaymentMethod : "Unknown"))
                .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => src.Payment != null ? src.Payment.PaymentDate : (DateTime?)null))
                .ForMember(dest => dest.Passengers, opt => opt.MapFrom(src => src.Passengers));
        }
    }
}