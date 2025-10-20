using Microsoft.EntityFrameworkCore;
using SkyReserve.Application.Repository;
using SkyReserve.Domain.Entities;
using SkyReserve.Infrastructure.Persistence;

namespace SkyReserve.Infrastructure.Repository.implementation
{
    public class RoleRepository : IRoleRepository
    {
        private readonly SkyReserveDbContext _context;

        public RoleRepository(SkyReserveDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationRole?> GetByIdAsync(string id)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> ExistsAsync(string name, string? excludeId = null)
        {
            var query = _context.Roles.Where(r => r.Name == name);

            if (!string.IsNullOrEmpty(excludeId))
            {
                query = query.Where(r => r.Id != excludeId);
            }

            return await query.AnyAsync();
        }
    }
}
