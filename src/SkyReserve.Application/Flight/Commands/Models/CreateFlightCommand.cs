using MediatR;
using SkyReserve.Application.Flight.DTOS;

namespace SkyReserve.Application.Flight.Commands.Models
{
    public class CreateFlightCommand : IRequest<FlightDto>
    {
        public string FlightNumber { get; set; } = string.Empty;
        public int DepartureAirportId { get; set; }
        public int ArrivalAirportId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string AircraftModel { get; set; } = string.Empty;
        public string Status { get; set; } = "Scheduled";
    }
}
