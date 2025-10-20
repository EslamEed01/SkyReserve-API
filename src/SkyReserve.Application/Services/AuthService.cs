using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkyReserve.Application.Consts;
using SkyReserve.Application.Contract.Authentication;
using SkyReserve.Application.Contract.Users;
using SkyReserve.Application.Interfaces;
using SkyReserve.Application.Repository;
using SkyReserve.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace SkyReserve.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtProvider _jwtProvider;
        private readonly ILogger<AuthService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly int _refreshTokenExpiryDays = 14;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly INotificationProducer _notificationProducer;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IJwtProvider jwtProvider,
            ILogger<AuthService> logger,
            IHttpContextAccessor httpContextAccessor,
            IUserRepository userRepository,
            IMapper mapper,
            SignInManager<ApplicationUser> signInManager,
            INotificationProducer notificationProducer)
        {
            _userManager = userManager;
            _jwtProvider = jwtProvider;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
            _mapper = mapper;
            _signInManager = signInManager;
            _notificationProducer = notificationProducer;
        }

        public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            if (await _userManager.FindByEmailAsync(email) is not { } user)
                return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

            if (user.IsDisabled)
                return Result.Failure<AuthResponse>(UserErrors.DisabledUser);

            var result = await _signInManager.PasswordSignInAsync(user, password, false, true);

            if (result.Succeeded)
            {
                var (userRoles, userPermissions) = await GetUserRolesAndPermissions(user, cancellationToken);

                var (token, expiresIn) = _jwtProvider.GenerateToken(user, userRoles, userPermissions);
                var refreshToken = GenerateRefreshToken();
                var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

                user.RefreshTokens.Add(new RefreshToken
                {
                    Token = refreshToken,
                    ExpiresOn = refreshTokenExpiration
                });

                await _userManager.UpdateAsync(user);

                await SendWelcomeSmsAsync(user);

                try
                {
                    await _notificationProducer.CreateLoginNotificationAsync(user.Id, cancellationToken);
                    _logger.LogInformation("Login notification queued for user {UserId}", user.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to queue login notification for user {UserId}. Login process continued normally.", user.Id);
                }

                var response = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, expiresIn, refreshToken, refreshTokenExpiration);

                return Result.Success(response);
            }

            var error = result.IsNotAllowed
                ? UserErrors.EmailNotConfirmed
                : result.IsLockedOut
                ? UserErrors.LockedUser
                : UserErrors.InvalidCredentials;

            return Result.Failure<AuthResponse>(error);
        }

        public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            var userId = _jwtProvider.ValidateToken(token);

            if (userId is null)
                return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);

            if (user.IsDisabled)
                return Result.Failure<AuthResponse>(UserErrors.DisabledUser);

            if (user.LockoutEnd > DateTime.UtcNow)
                return Result.Failure<AuthResponse>(UserErrors.LockedUser);

            var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

            if (userRefreshToken is null)
                return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);

            userRefreshToken.RevokedOn = DateTime.UtcNow;

            var (userRoles, userPermissions) = await GetUserRolesAndPermissions(user, cancellationToken);

            var (newToken, expiresIn) = _jwtProvider.GenerateToken(user, userRoles, userPermissions);
            var newRefreshToken = GenerateRefreshToken();
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                ExpiresOn = refreshTokenExpiration
            });

            await _userManager.UpdateAsync(user);

            var response = new AuthResponse(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                newToken,
                expiresIn,
                newRefreshToken,
                refreshTokenExpiration
            );

            return Result.Success(response);
        }

        public async Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            var userId = _jwtProvider.ValidateToken(token);

            if (userId is null)
                return Result.Failure(UserErrors.InvalidJwtToken);

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                return Result.Failure(UserErrors.InvalidJwtToken);

            var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

            if (userRefreshToken is null)
                return Result.Failure(UserErrors.InvalidRefreshToken);

            userRefreshToken.RevokedOn = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            return Result.Success();
        }

        private static string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            var emailIsExists = await _userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);

            if (emailIsExists)
                return Result.Failure(UserErrors.DuplicatedEmail);

            var user = _mapper.Map<ApplicationUser>(request);
            user.Id = Guid.NewGuid().ToString();
            user.RoleType = "student";

            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                _logger.LogInformation("Confirmation code: {code}", code);

                await SendConfirmationEmail(user, code);

                try
                {
                    await _notificationProducer.CreateRegistrationWelcomeNotificationAsync(user.Id, cancellationToken);
                    _logger.LogInformation("Registration welcome notification queued for user {UserId}", user.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to queue registration welcome notification for user {UserId}. Registration process continued normally.", user.Id);
                }

                return Result.Success();
            }

            var error = result.Errors.First();

            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request)
        {
            if (await _userManager.FindByIdAsync(request.UserId) is not { } user)
                return Result.Failure(UserErrors.InvalidCode);

            if (user.EmailConfirmed)
                return Result.Failure(UserErrors.DuplicatedConfirmation);

            var code = request.Code;

            try
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            }
            catch (FormatException)
            {
                return Result.Failure(UserErrors.InvalidCode);
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, DefaultRoles.passenger);
                return Result.Success();
            }

            var error = result.Errors.First();

            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        public async Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request)
        {
            if (await _userManager.FindByEmailAsync(request.Email) is not { } user)
                return Result.Success();

            if (user.EmailConfirmed)
                return Result.Failure(UserErrors.DuplicatedConfirmation);

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            _logger.LogInformation("Confirmation code: {code}", code);

            await SendConfirmationEmail(user, code);

            return Result.Success();
        }

        public async Task<Result> SendResetPasswordCodeAsync(string email)
        {
            if (await _userManager.FindByEmailAsync(email) is not { } user)
                return Result.Success();

            if (!user.EmailConfirmed)
                return Result.Failure(UserErrors.EmailNotConfirmed);

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            _logger.LogInformation("Reset code: {code}", code);

            await SendResetPasswordEmail(user, code);

            return Result.Success();
        }

        public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null || !user.EmailConfirmed)
                return Result.Failure(UserErrors.InvalidCode);

            IdentityResult result;

            try
            {
                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
                result = await _userManager.ResetPasswordAsync(user, code, request.NewPassword);
            }
            catch (FormatException)
            {
                result = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
            }

            if (result.Succeeded)
                return Result.Success();

            var error = result.Errors.First();

            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status401Unauthorized));
        }

        private async Task SendConfirmationEmail(ApplicationUser user, string code)
        {
            try
            {
                await _notificationProducer.CreateEmailConfirmationNotificationAsync(user.Id, code);
                _logger.LogInformation("Email confirmation notification queued for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to queue email confirmation notification for user {UserId}", user.Id);
                throw;
            }
        }

        public async Task SendResetPasswordEmail(ApplicationUser user, string code)
        {
            try
            {
                await _notificationProducer.CreatePasswordResetNotificationAsync(user.Id, code);
                _logger.LogInformation("Password reset notification queued for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to queue password reset notification for user {UserId}", user.Id);
                throw;
            }
        }

        private async Task SendWelcomeSmsAsync(ApplicationUser user)
        {
            try
            {
                if (!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    await _notificationProducer.CreateLoginNotificationAsync(user.Id);
                    _logger.LogInformation("Welcome SMS notification queued for user {UserId} at {PhoneNumber}", user.Id, user.PhoneNumber);
                }
                else
                {
                    _logger.LogInformation("No phone number available for user {UserId}, skipping welcome SMS", user.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to queue welcome SMS notification for user {UserId}. Login process continued normally.", user.Id);
            }
        }

        private async Task<(IEnumerable<string> roles, IEnumerable<string> permissions)> GetUserRolesAndPermissions(ApplicationUser user, CancellationToken cancellationToken)
        {
            return await _userRepository.GetUserRolesAndPermissionsAsync(user.Id, cancellationToken);
        }

        Task IAuthService.SendConfirmationEmail(ApplicationUser user, string code)
        {
            return SendConfirmationEmail(user, code);
        }

        Task<(IEnumerable<string> roles, IEnumerable<string> permissions)> IAuthService.GetUserRolesAndPermissions(ApplicationUser user, CancellationToken cancellationToken)
        {
            return GetUserRolesAndPermissions(user, cancellationToken);
        }
    }
}
