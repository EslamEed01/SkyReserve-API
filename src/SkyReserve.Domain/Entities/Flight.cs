namespace SkyReserve.Domain.Entities
{
    public class Flight
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; }
        public int DepartureAirportId { get; set; }
        public int ArrivalAirportId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string AircraftModel { get; set; }
        public string Status { get; set; }
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Airport DepartureAirport { get; set; }
        public Airport ArrivalAirport { get; set; }
        public ICollection<Seat> Seats { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Price> Prices { get; set; } = new List<Price>();
    }
}
