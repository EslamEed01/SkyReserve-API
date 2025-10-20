using SkyReserve.Domain.Entities;

namespace SkyReserve.Application.Repository
{
    public interface IRoleRepository
    {
        Task<ApplicationRole?> GetByIdAsync(string id);
        Task<bool> ExistsAsync(string name, string? excludeId = null);
    }
}
