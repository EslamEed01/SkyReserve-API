using MediatR;

namespace SkyReserve.Application.Flight.Commands.Models
{
    public class DeleteFlightCommand : IRequest<bool>
    {
        public int FlightId { get; set; }
    }
}
