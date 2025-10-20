namespace SkyReserve.Domain.Entities
{
    public class Booking
    {
        public int BookingId { get; set; }
        public string BookingRef { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public int FlightId { get; set; }
        public int PriceId { get; set; }
        public string FareClass { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public int PaymentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; }
        public Flight Flight { get; set; }
        public Price Price { get; set; }
        public Payment Payment { get; set; }
        public ICollection<Passenger> Passengers { get; set; }
    }
}
