using Microsoft.AspNetCore.Identity;

namespace SkyReserve.Domain.Entities
{
    public class ApplicationRole : IdentityRole
    {
        public bool IsDefault { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

    }
}
