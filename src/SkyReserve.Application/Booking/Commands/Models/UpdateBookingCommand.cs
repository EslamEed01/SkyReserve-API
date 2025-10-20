using MediatR;
using SkyReserve.Application.Booking.DTOS;

namespace SkyReserve.Application.Booking.Commands.Models
{
    public class UpdateBookingCommand : IRequest<BookingDto>
    {
        public int BookingId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}