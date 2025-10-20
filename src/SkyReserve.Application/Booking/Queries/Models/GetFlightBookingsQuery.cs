using MediatR;
using SkyReserve.Application.Booking.DTOS;

namespace SkyReserve.Application.Booking.Queries.Models
{
    public class GetFlightBookingsQuery : IRequest<IEnumerable<BookingDto>>
    {
        public int FlightId { get; set; }
    }
}