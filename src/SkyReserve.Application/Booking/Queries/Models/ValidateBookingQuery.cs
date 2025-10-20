using MediatR;

namespace SkyReserve.Application.Booking.Queries.Models
{
    public class ValidateBookingQuery : IRequest<bool>
    {
        public int BookingId { get; set; }
    }
}