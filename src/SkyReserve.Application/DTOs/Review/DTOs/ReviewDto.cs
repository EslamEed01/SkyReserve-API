namespace SkyReserve.Application.DTOs.Review.DTOs
{
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int FlightId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? UserName { get; set; }
        public string? FlightNumber { get; set; }
        public string? DepartureCity { get; set; }
        public string? ArrivalCity { get; set; }
    }
}