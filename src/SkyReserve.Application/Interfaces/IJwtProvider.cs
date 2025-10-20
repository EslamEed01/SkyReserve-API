using SkyReserve.Domain.Entities;

namespace SkyReserve.Application.Interfaces
{
    public interface IJwtProvider
    {
        (string token, int expiresIn) GenerateToken(ApplicationUser user, IEnumerable<string> roles, IEnumerable<string> permissions);
        RefreshToken GenerateRefreshToken();
        string? ValidateToken(string token);
    }
}
