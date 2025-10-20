using System.ComponentModel.DataAnnotations;

namespace SkyReserve.Application.DTOs.Review.DTOs
{
    public class CreateReviewDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int FlightId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Comment must be between 10 and 1000 characters")]
        public string Comment { get; set; } = string.Empty;
    }
}