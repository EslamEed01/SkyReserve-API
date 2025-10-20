using MediatR;
using SkyReserve.Application.Flight.DTOS;

namespace SkyReserve.Application.Flight.Commands.Models
{
    public class UpdateFlightCommand : IRequest<FlightDto>
    {
        public int FlightId { get; set; }
        public string? FlightNumber { get; set; }
        public int? DepartureAirportId { get; set; }
        public int? ArrivalAirportId { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public string? AircraftModel { get; set; }
        public string? Status { get; set; }
    }
}
