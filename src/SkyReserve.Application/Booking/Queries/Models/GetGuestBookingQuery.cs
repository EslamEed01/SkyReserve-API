using MediatR;
using SkyReserve.Application.Booking.DTOS;

namespace SkyReserve.Application.Booking.Queries.Models
{
    public class GetGuestBookingQuery : IRequest<BookingDto?>
    {
        public string BookingRef { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}