namespace SkyReserve.Application.Contract.OAuth
{
    public class GoogleOAuthUrlResponse
    {
        public string AuthorizationUrl { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

  
}