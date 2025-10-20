using MediatR;
using SkyReserve.Application.Booking.DTOS;

namespace SkyReserve.Application.Booking.Queries.Models
{
    public class GetUserBookingsQuery : IRequest<IEnumerable<BookingDto>>
    {
        public string UserId { get; set; } = string.Empty;
    }
}