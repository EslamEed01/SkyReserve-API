namespace SkyReserve.Application.Settings
{
    public class GoogleCalendarSettings
    {
        public const string SectionName = "GoogleCalendar";

        public string ApiBaseUrl { get; set; } = "https://www.googleapis.com/calendar/v3";
        public string DefaultCalendarId { get; set; } = "primary";
        public string TimeZone { get; set; } = "UTC";
    }
}
