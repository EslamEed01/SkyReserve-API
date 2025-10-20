using SkyReserve.Domain.Entities;

namespace Learnova.Business.DTOs.Contract.Users
{
    public class UserWithRoles
    {
        public ApplicationUser User { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}
