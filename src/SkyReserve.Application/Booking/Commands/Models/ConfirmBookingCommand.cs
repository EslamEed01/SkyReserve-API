using MediatR;

namespace SkyReserve.Application.Booking.Commands.Models
{
    public class ConfirmBookingCommand : IRequest<bool>
    {
        public int BookingId { get; set; }
    }
}