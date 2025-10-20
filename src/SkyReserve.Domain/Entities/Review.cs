namespace SkyReserve.Domain.Entities
{
    public class Review
    {

        public int ReviewId { get; set; }
        public string UserId { get; set; }
        public int FlightId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; }
        public Flight Flight { get; set; }
    }
}
