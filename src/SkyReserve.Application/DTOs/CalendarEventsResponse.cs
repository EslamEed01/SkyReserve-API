namespace SkyReserve.Application.DTOs
{
    public class CalendarEventsResponse
    {
        public bool IsSuccess { get; set; }
        public List<CalendarEvent> Events { get; set; } = new();
        public string? Error { get; set; }
        public string? ErrorDescription { get; set; }
    }
}
