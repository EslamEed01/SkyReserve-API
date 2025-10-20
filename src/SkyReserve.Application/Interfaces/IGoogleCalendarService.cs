using SkyReserve.Application.DTOs;

namespace SkyReserve.Application.Interfaces
{
    public interface IGoogleCalendarService
    {
        
        Task<CalendarEventResponse> CreateEventAsync(string accessToken, CreateCalendarEventRequest request);
        Task<CalendarEventsResponse> GetEventsAsync(string accessToken, DateTime? startDate = null, DateTime? endDate = null);
        Task<CalendarEventResponse> DeleteEventAsync(string accessToken, string eventId);
        Task<CalendarEventResponse> CreateFlightEventAsync(string accessToken, string flightNumber, DateTime departureTime, DateTime arrivalTime, string from, string to);
        Task<BookingCalendarResponse> AddBookingToCalendarAsync(string accessToken, string bookingRef);
    }
}