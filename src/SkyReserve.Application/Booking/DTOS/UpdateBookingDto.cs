using System.ComponentModel.DataAnnotations;

namespace SkyReserve.Application.Booking.DTOS
{
    public class UpdateBookingDto
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than 0")]
        public decimal TotalAmount { get; set; }
    }
}