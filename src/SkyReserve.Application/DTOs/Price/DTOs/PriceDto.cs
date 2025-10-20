namespace SkyReserve.Application.DTOs.Price.DTOs
{
    public class PriceDto
    {
        public int Id { get; set; }
        public int FlightId { get; set; }
        public string FareClass { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? FlightNumber { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
    }
}