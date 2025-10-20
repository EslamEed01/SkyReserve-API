using SkyReserve.Application.Consts;
using SkyReserve.Application.Contract.Authentication;
using SkyReserve.Application.Contract.Users;
using SkyReserve.Domain.Entities;

namespace SkyReserve.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default);
        Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
        Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);


        Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);


        Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request);

        Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request);

        Task<Result> SendResetPasswordCodeAsync(string email);


        Task<Result> ResetPasswordAsync(ResetPasswordRequest request);


        Task SendConfirmationEmail(ApplicationUser user, string code);


        Task SendResetPasswordEmail(ApplicationUser user, string code);


        Task<(IEnumerable<string> roles, IEnumerable<string> permissions)> GetUserRolesAndPermissions(ApplicationUser user, CancellationToken cancellationToken);

    }
}
