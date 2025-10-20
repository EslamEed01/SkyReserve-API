using MediatR;
using SkyReserve.Application.Flight.DTOS;

namespace SkyReserve.Application.Flight.Queries.Models
{
    public class GetFlightsByRouteQuery : IRequest<IEnumerable<FlightDto>>
    {
        public int DepartureAirportId { get; set; }
        public int ArrivalAirportId { get; set; }
        public DateTime DepartureDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
