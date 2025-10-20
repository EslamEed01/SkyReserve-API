using System.ComponentModel.DataAnnotations;

namespace SkyReserve.Application.DTOs.Payment.DTOs
{
    public class CreatePaymentIntentRequest
    {

        [Required]
        [Range(50, 99999999, ErrorMessage = "Amount must be between $0.50 and $999,999.99")]
        public long Amount { get; set; }


        public string Currency { get; set; } = "usd";

        public int? BookingId { get; set; }

        [EmailAddress(ErrorMessage = "Please provide a valid email address")]
        public string? CustomerEmail { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }
    }
}