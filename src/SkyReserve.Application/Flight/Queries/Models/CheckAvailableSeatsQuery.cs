using MediatR;

namespace SkyReserve.Application.Flight.Queries.Models
{
    public class CheckAvailableSeatsQuery : IRequest<bool>
    {
        public int FlightId { get; set; }
        public int RequiredSeats { get; set; }
    }
}