using System.ComponentModel.DataAnnotations;

namespace SkyReserve.Application.DTOs
{
    public class CreateCalendarEventRequest
    {
        [Required]
        public string Summary { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        public string Location { get; set; } = string.Empty;

        public List<string> AttendeeEmails { get; set; } = new();
    }
}
