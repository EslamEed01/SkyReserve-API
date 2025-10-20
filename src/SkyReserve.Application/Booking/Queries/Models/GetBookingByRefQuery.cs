using MediatR;
using SkyReserve.Application.Booking.DTOS;

namespace SkyReserve.Application.Booking.Queries.Models
{
    public class GetBookingByRefQuery : IRequest<BookingDto?>
    {
        public string BookingRef { get; set; } = string.Empty;
    }
}