namespace SkyReserve.Domain.Entities
{
    public class Airport
    {
        public int AirportId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Flight> DepartingFlights { get; set; }
        public ICollection<Flight> ArrivingFlights { get; set; }
    }
}
