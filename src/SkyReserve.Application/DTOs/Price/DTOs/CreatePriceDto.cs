using System.ComponentModel.DataAnnotations;

namespace SkyReserve.Application.DTOs.Price.DTOs
{
    public class CreatePriceDto
    {
        [Required]
        public int FlightId { get; set; }

        [Required]
        [StringLength(50)]
        public string FareClass { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Base price must be greater than 0")]
        public decimal BasePrice { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; } = "USD";

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }
    }
}