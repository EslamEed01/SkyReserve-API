using MediatR;
using SkyReserve.Application.Flight.DTOS;

namespace SkyReserve.Application.Flight.Queries.Models
{
    public class GetFlightByIdQuery : IRequest<FlightDto?>
    {
        public int FlightId { get; set; }
    }
}
