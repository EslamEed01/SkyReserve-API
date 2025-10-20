using System.ComponentModel.DataAnnotations;

namespace SkyReserve.Application.DTOs
{
    public class BookingCalendarRequest
    {
        [Required]
        public string BookingRef { get; set; } = string.Empty;
    }
}
