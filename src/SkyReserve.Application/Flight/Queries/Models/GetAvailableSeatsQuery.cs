using MediatR;

namespace SkyReserve.Application.Flight.Queries.Models
{
    public class GetAvailableSeatsQuery : IRequest<int>
    {
        public int FlightId { get; set; }
    }
}