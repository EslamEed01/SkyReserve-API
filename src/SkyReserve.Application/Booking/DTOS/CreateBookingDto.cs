using System.ComponentModel.DataAnnotations;

namespace SkyReserve.Application.Booking.DTOS
{
    public class CreateBookingDto
    {
        public string? UserId { get; set; }

        [Required]
        public int FlightId { get; set; }

        [Required]
        public int PriceId { get; set; }

        [Required]
        [StringLength(50)]
        public string FareClass { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string BookingRef { get; set; } = string.Empty;
    }
}