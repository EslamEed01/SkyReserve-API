using SkyReserve.Application.Contract.OAuth;

namespace SkyReserve.Application.Interfaces
{
    public interface IGoogleOAuthService
    {
  
        Task<GoogleOAuthUrlResponse> GenerateAuthorizationUrlAsync(GoogleOAuthUrlRequest request);
        Task<GoogleOAuthCallbackResponse> HandleCallbackAsync(GoogleOAuthCallbackRequest request);
        Task<GoogleUserInfo?> GetUserInfoAsync(string accessToken);
        bool ValidateState(string state);
    }
}