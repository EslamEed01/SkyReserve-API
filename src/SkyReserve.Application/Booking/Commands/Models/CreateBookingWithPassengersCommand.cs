using MediatR;
using SkyReserve.Application.Booking.DTOS;
using SkyReserve.Application.Passenger.DTOS;

namespace SkyReserve.Application.Booking.Commands.Models
{
    public class CreateBookingWithPassengersCommand : IRequest<BookingDto>
    {
        public string UserId { get; set; } = string.Empty;
        public int FlightId { get; set; }
        public string FareClass { get; set; } = "Economy";
        public List<CreatePassengerDto> Passengers { get; set; } = new();
    }
}