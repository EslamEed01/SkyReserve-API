using SkyReserve.Application.Passenger.DTOS;

namespace SkyReserve.Application.Booking.DTOS
{
    public class BookingDto
    {
        public int BookingId { get; set; }
        public string BookingRef { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public int FlightId { get; set; }
        public int PriceId { get; set; }
        public string FareClass { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int PaymentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string FlightNumber { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string DepartureAirportCode { get; set; } = string.Empty;
        public string DepartureAirportName { get; set; } = string.Empty;
        public string ArrivalAirportCode { get; set; } = string.Empty;
        public string ArrivalAirportName { get; set; } = string.Empty;

        // Price 
        public decimal BasePrice { get; set; }
        public string Currency { get; set; } = string.Empty;

        // User information
        public string? UserFirstName { get; set; }
        public string? UserLastName { get; set; }
        public string? UserEmail { get; set; }
        public ICollection<PassengerDto>? Passengers { get; set; }
    }
}