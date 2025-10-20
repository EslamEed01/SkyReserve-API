using MediatR;
using SkyReserve.Application.Passenger.DTOS;

namespace SkyReserve.Application.Passenger.Queries.Models
{
    public class GetGuestBookingDetailsQuery : IRequest<GuestBookingDetailsDto?>
    {
        public string BookingRef { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}