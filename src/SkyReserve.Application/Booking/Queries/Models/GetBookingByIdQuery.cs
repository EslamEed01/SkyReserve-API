using MediatR;
using SkyReserve.Application.Booking.DTOS;

namespace SkyReserve.Application.Booking.Queries.Models
{
    public class GetBookingByIdQuery : IRequest<BookingDto?>
    {
        public int BookingId { get; set; }
    }
}