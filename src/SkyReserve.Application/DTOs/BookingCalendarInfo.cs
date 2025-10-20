namespace SkyReserve.Application.DTOs
{
    public class BookingCalendarInfo
    {
        public string BookingRef { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
    }
}
