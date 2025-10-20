using System.ComponentModel.DataAnnotations;

namespace SkyReserve.Domain.Entities
{
    public class NotificationChannel
    {
        public int ChannelId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ChannelType { get; set; }

        [Required]
        [MaxLength(100)]
        public string ChannelName { get; set; }

        [MaxLength(255)]
        public string? Configuration { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<Notification> Notifications { get; set; } = [];
    }
}