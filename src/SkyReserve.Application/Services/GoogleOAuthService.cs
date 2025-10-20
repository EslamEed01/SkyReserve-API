using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkyReserve.Application.Contract.OAuth;
using SkyReserve.Application.Interfaces;
using SkyReserve.Application.Repository;
using SkyReserve.Application.Settings;
using SkyReserve.Domain.Entities;
using System.Security.Cryptography;
using System.Text.Json;

namespace SkyReserve.Infrastructure.Services
{
    public class GoogleOAuthService : IGoogleOAuthService
    {
        private readonly GoogleOAuthSettings _settings;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRedisService _redisService;
        private readonly IJwtProvider _jwtProvider;
        private readonly ILogger<GoogleOAuthService> _logger;
        private readonly HttpClient _httpClient;

        public GoogleOAuthService(
            IOptions<GoogleOAuthSettings> settings,
            IUserRepository userRepository,
            UserManager<ApplicationUser> userManager,
            IRedisService redisService,
            IJwtProvider jwtProvider,
            ILogger<GoogleOAuthService> logger,
            HttpClient httpClient)
        {
            _settings = settings.Value;
            _userRepository = userRepository;
            _userManager = userManager;
            _redisService = redisService;
            _jwtProvider = jwtProvider;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<GoogleOAuthUrlResponse> GenerateAuthorizationUrlAsync(GoogleOAuthUrlRequest request)
        {
            try
            {
                var state = GenerateSecureState();

                await _redisService.SetAsync($"oauth_state:{state}",
                    new { ReturnUrl = request.ReturnUrl, CreatedAt = DateTime.UtcNow },
                    TimeSpan.FromMinutes(10));

                var scopes = string.Join(" ", _settings.Scopes);

                var authUrl = $"{_settings.AuthorizationEndpoint}?" +
                    $"client_id={Uri.EscapeDataString(_settings.ClientId)}&" +
                    $"redirect_uri={Uri.EscapeDataString(_settings.RedirectUri)}&" +
                    $"scope={Uri.EscapeDataString(scopes)}&" +
                    $"response_type=code&" +
                    $"state={Uri.EscapeDataString(state)}&" +
                    $"access_type=offline&" +
                    $"prompt=consent";

                return new GoogleOAuthUrlResponse
                {
                    AuthorizationUrl = authUrl,
                    State = state,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate Google OAuth URL");
                throw;
            }
        }

        public async Task<GoogleOAuthCallbackResponse> HandleCallbackAsync(GoogleOAuthCallbackRequest request)
        {
            try
            {
                if (!ValidateState(request.State))
                {
                    return new GoogleOAuthCallbackResponse
                    {
                        IsSuccess = false,
                        Error = "invalid_state",
                        ErrorDescription = "Invalid or expired state parameter"
                    };
                }

                var tokenResponse = await ExchangeCodeForTokenAsync(request.Code);
                if (tokenResponse == null)
                {
                    return new GoogleOAuthCallbackResponse
                    {
                        IsSuccess = false,
                        Error = "token_exchange_failed",
                        ErrorDescription = "Failed to exchange authorization code for access token"
                    };
                }

                var userInfo = await GetUserInfoAsync(tokenResponse.AccessToken);
                if (userInfo == null)
                {
                    return new GoogleOAuthCallbackResponse
                    {
                        IsSuccess = false,
                        Error = "user_info_failed",
                        ErrorDescription = "Failed to retrieve user information from Google"
                    };
                }

                var user = await _userRepository.GetByEmailAsync(userInfo.Email);
                bool isNewUser = false;

                if (user == null)
                {
                    user = await CreateUserFromGoogleInfoAsync(userInfo);
                    isNewUser = true;
                }

                var (roles, permissions) = await _userRepository.GetUserRolesAndPermissionsAsync(user.Id, CancellationToken.None);

                var tokenResult = _jwtProvider.GenerateToken(user, roles, permissions);
                var refreshToken = _jwtProvider.GenerateRefreshToken();

                await _redisService.DeleteAsync($"oauth_state:{request.State}");

                return new GoogleOAuthCallbackResponse
                {
                    IsSuccess = true,
                    JwtToken = tokenResult.token,
                    RefreshToken = refreshToken.Token,
                    ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResult.expiresIn),
                    UserInfo = userInfo,
                    RequiresRegistration = isNewUser
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Google OAuth callback");
                return new GoogleOAuthCallbackResponse
                {
                    IsSuccess = false,
                    Error = "internal_error",
                    ErrorDescription = "An internal error occurred during authentication"
                };
            }
        }

        public async Task<GoogleUserInfo?> GetUserInfoAsync(string accessToken)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, _settings.UserInfoEndpoint);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get user info from Google. Status: {StatusCode}", response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<GoogleUserInfo>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user info from Google");
                return null;
            }
        }

        public bool ValidateState(string state)
        {
            if (string.IsNullOrEmpty(state)) return false;

            try
            {
                var stateData = _redisService.GetAsync<object>($"oauth_state:{state}").GetAwaiter().GetResult();
                return stateData != null;
            }
            catch
            {
                return false;
            }
        }

        private async Task<GoogleTokenResponse?> ExchangeCodeForTokenAsync(string code)
        {
            try
            {
                var parameters = new Dictionary<string, string>
                {
                    ["client_id"] = _settings.ClientId,
                    ["client_secret"] = _settings.ClientSecret,
                    ["code"] = code,
                    ["grant_type"] = "authorization_code",
                    ["redirect_uri"] = _settings.RedirectUri
                };

                var content = new FormUrlEncodedContent(parameters);
                var response = await _httpClient.PostAsync(_settings.TokenEndpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Token exchange failed. Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, errorContent);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<GoogleTokenResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token exchange");
                return null;
            }
        }

        private async Task<ApplicationUser> CreateUserFromGoogleInfoAsync(GoogleUserInfo googleUser)
        {
            try
            {
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = googleUser.Email,
                    Email = googleUser.Email,
                    NormalizedEmail = googleUser.Email.ToUpperInvariant(),
                    NormalizedUserName = googleUser.Email.ToUpperInvariant(),
                    EmailConfirmed = googleUser.VerifiedEmail,
                    FirstName = googleUser.GivenName ?? googleUser.Name.Split(' ').FirstOrDefault() ?? "Unknown",
                    LastName = googleUser.FamilyName ?? googleUser.Name.Split(' ').LastOrDefault() ?? "User",
                    ProfilePictureUrl = googleUser.Picture,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    RoleType = "passenger",
                    DateOfBirth = DateTime.UtcNow.AddYears(-18),
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };

              
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                   
                    await _userManager.AddToRoleAsync(user, "User");
                    _logger.LogInformation("Created new user from Google OAuth: {Email}", user.Email);
                    return user;
                }
                else
                {
                    _logger.LogError("Failed to create user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user from Google info");
                throw;
            }
        }

        private string GenerateSecureState()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}