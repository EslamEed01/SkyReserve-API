namespace SkyReserve.Domain.Entities
{
    public class Seat
    {
        public int SeatId { get; set; }
        public int FlightId { get; set; }
        public required string SeatNumber { get; set; }
        public required string Class { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public required Flight Flight { get; set; }
    }
}
