namespace SkyReserve.Domain.Entities
{
    public class Price
    {
        public int Id { get; set; }
        public int FlightId { get; set; }
        public string FareClass { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Flight Flight { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}