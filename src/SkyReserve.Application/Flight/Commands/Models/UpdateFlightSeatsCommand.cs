using MediatR;

namespace SkyReserve.Application.Flight.Commands.Models
{
    public class UpdateFlightSeatsCommand : IRequest<bool>
    {
        public int FlightId { get; set; }
        public int SeatChange { get; set; }
    }
}