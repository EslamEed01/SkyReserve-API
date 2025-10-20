using Microsoft.AspNetCore.Mvc;
using SkyReserve.Application.Consts;
using SkyReserve.Application.Contract.Authentication;
using SkyReserve.Application.Contract.OAuth;
using SkyReserve.Application.Contract.Users;
using SkyReserve.Application.Interfaces;

namespace SkyReserve.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService, IGoogleOAuthService googleOAuthService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly IGoogleOAuthService _googleOAuthService = googleOAuthService;

        /// <summary>
        /// Authenticate user and return JWT token  
        /// </summary>
        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var authResult = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);

            return authResult.IsSuccess ? Ok(authResult.Value) : BadRequest("Invalid email/password");
        }

        /// <summary>
        /// Refresh JWT token using refresh token  
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var authResult = await _authService.GetRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);

            return authResult.IsSuccess ? Ok(authResult.Value) : BadRequest("Invalid token");
        }

        /// <summary>
        /// Revoke refresh token  
        /// </summary>
        [HttpPost("revoke-refresh-token")]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.RevokeRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);

            return result.IsSuccess ? Ok() : result.ToProblem();
        }

        /// <summary>
        /// Register a new user  
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(request, cancellationToken);

            return result.IsSuccess ? Ok() : result.ToProblem();
        }

        /// <summary>
        /// Confirm email address   
        /// </summary>
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.ConfirmEmailAsync(request);

            return result.IsSuccess ? Ok() : result.ToProblem();
        }

        /// <summary>
        /// Resend confirmation email  
        /// </summary>
        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.ResendConfirmationEmailAsync(request);

            return result.IsSuccess ? Ok() : result.ToProblem();
        }

        /// <summary>
        /// Send forget password code to email  
        /// </summary>
        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
        {
            var result = await _authService.SendResetPasswordCodeAsync(request.Email);

            return result.IsSuccess ? Ok() : result.ToProblem();
        }

        /// <summary>
        /// Reset password using code sent to email  
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _authService.ResetPasswordAsync(request);

            return result.IsSuccess ? Ok() : result.ToProblem();
        }


        /// <summary>
        /// Generate Google OAuth authorization URL (Redirect URL)
        /// </summary>
        [HttpGet("google/login")]
        public async Task<IActionResult> GoogleLogin([FromQuery] string? returnUrl = null)
        {
            try
            {
                var request = new GoogleOAuthUrlRequest { ReturnUrl = returnUrl };
                var result = await _googleOAuthService.GenerateAuthorizationUrlAsync(request);

                return Ok(new
                {
                    redirectUrl = result.AuthorizationUrl,
                    state = result.State,
                    expiresAt = result.ExpiresAt
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to generate OAuth URL", details = ex.Message });
            }
        }

        /// <summary>
        /// Handle Google OAuth callback - Complete authentication flow
        /// </summary>
        [HttpGet("google/callback")]
        public async Task<IActionResult> GoogleCallback(
            [FromQuery] string? code = null,
            [FromQuery] string? state = null,
            [FromQuery] string? error = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(error))
                {
                    return BadRequest(new { error = error, message = "Google OAuth authentication failed" });
                }

                if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
                {
                    return BadRequest(new { error = "missing_parameters", message = "Authorization code or state parameter is missing" });
                }

                var callbackRequest = new GoogleOAuthCallbackRequest
                {
                    Code = code,
                    State = state
                };

                var result = await _googleOAuthService.HandleCallbackAsync(callbackRequest);

                if (!result.IsSuccess)
                {
                    return BadRequest(new { error = result.Error, message = result.ErrorDescription });
                }

                return Ok(new
                {
                    success = true,
                    token = result.JwtToken,
                    refreshToken = result.RefreshToken,
                    expiresAt = result.ExpiresAt,
                    user = new
                    {
                        email = result.UserInfo?.Email,
                        name = result.UserInfo?.Name,
                        picture = result.UserInfo?.Picture,
                        firstName = result.UserInfo?.GivenName,
                        lastName = result.UserInfo?.FamilyName
                    },
                    isNewUser = result.RequiresRegistration
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "internal_error", message = "An error occurred during authentication", details = ex.Message });
            }
        }

    }
}
