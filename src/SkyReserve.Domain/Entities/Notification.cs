using System.ComponentModel.DataAnnotations;

namespace SkyReserve.Domain.Entities
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }    
        public int ChannelId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Recipient { get; set; }

        public int? TemplateId { get; set; }
        public string? UserId { get; set; }

        [Required]
        public string Payload { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";
        public int RetryCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SentAt { get; set; }

        // Navigation property
        public NotificationChannel Channel { get; set; }
        public ApplicationUser User { get; set; }


    }
}
