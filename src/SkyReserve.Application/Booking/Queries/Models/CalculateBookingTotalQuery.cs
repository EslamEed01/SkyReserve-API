using MediatR;

namespace SkyReserve.Application.Booking.Queries.Models
{
    public class CalculateBookingTotalQuery : IRequest<decimal>
    {
        public int FlightId { get; set; }
        public int PassengerCount { get; set; }
        public string? FareClass { get; set; } = "Economy";
    }
}