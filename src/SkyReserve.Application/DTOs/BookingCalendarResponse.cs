namespace SkyReserve.Application.DTOs
{
    public class BookingCalendarResponse
    {
        public bool IsSuccess { get; set; }
        public string? EventId { get; set; }
        public string? EventUrl { get; set; }
        public string? Error { get; set; }
        public string? ErrorDescription { get; set; }
        public BookingCalendarInfo? BookingInfo { get; set; }
    }
}
