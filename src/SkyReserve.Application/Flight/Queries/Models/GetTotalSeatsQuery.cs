using MediatR;

namespace SkyReserve.Application.Flight.Queries.Models
{
    public class GetTotalSeatsQuery : IRequest<int>
    {
        public int FlightId { get; set; }
    }
}