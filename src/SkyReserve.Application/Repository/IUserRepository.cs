using SkyReserve.Domain.Entities;

namespace SkyReserve.Application.Repository
{
    public interface IUserRepository
    {
        Task<(IEnumerable<string> roles, IEnumerable<string> permissions)> GetUserRolesAndPermissionsAsync(string userId, CancellationToken cancellationToken);
        Task<ApplicationUser?> GetByEmailAsync(string email);
        Task<ApplicationUser> CreateAsync(ApplicationUser user);
        Task<ApplicationUser?> GetByIdAsync(string userId);
    }
}
