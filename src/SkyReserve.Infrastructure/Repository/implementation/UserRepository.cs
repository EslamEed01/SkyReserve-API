using Microsoft.EntityFrameworkCore;
using SkyReserve.Application.Repository;
using SkyReserve.Domain.Entities;
using SkyReserve.Infrastructure.Persistence;

namespace SkyReserve.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly SkyReserveDbContext _context;

        public UserRepository(SkyReserveDbContext context)
        {
            _context = context;
        }


        public async Task<(IEnumerable<string> roles, IEnumerable<string> permissions)> GetUserRolesAndPermissionsAsync(string userId, CancellationToken cancellationToken)
        {
            var userRoles = await (from ur in _context.UserRoles
                                   join r in _context.Roles on ur.RoleId equals r.Id
                                   where ur.UserId == userId
                                   select r.Name!)
                                   .ToListAsync(cancellationToken);

            var userPermissions = await (from r in _context.Roles
                                         join p in _context.RoleClaims on r.Id equals p.RoleId
                                         where userRoles.Contains(r.Name!)
                                         select p.ClaimValue!)
                                         .Distinct()
                                         .ToListAsync(cancellationToken);

            return (userRoles, userPermissions);
        }

        public async Task<ApplicationUser?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<ApplicationUser> CreateAsync(ApplicationUser user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<ApplicationUser?> GetByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }
    }
}
