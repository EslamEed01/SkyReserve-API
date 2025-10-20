using MediatR;
using SkyReserve.Application.Booking.DTOS;

namespace SkyReserve.Application.Booking.Commands.Models
{
    public class CreateGuestBookingCommand : IRequest<BookingDto>
    {
        public int FlightId { get; set; }
        public string FareClass { get; set; } = "Economy";
        public List<GuestPassengerDto> Passengers { get; set; } = new();
    }


}