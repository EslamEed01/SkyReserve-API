using Microsoft.AspNetCore.Identity;

namespace SkyReserve.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDisabled { get; set; }
        public string RoleType { get; set; } = string.Empty;
        public List<RefreshToken> RefreshTokens { get; set; } = [];

        public ICollection<Booking> Bookings { get; set; } = [];
        public ICollection<Notification> Notifications { get; set; } = [];
        public ICollection<Review> Reviews { get; set; } = [];
        public ICollection<Passenger> Passengers { get; set; } = [];

    }
}
