using MediatR;

namespace SkyReserve.Application.Booking.Commands.Models
{
    public class CancelBookingCommand : IRequest<bool>
    {
        public int BookingId { get; set; }
        public string CancellationReason { get; set; } = string.Empty;
    }
}