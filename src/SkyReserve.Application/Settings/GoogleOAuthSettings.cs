namespace SkyReserve.Application.Settings
{
    public class GoogleOAuthSettings
    {
        public const string SectionName = "GoogleOAuth";

        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string[] Scopes { get; set; } = ["openid", "profile", "email"];
        public string AuthorizationEndpoint { get; set; } = "https://accounts.google.com/o/oauth2/v2/auth";
        public string TokenEndpoint { get; set; } = "https://oauth2.googleapis.com/token";
        public string UserInfoEndpoint { get; set; } = "https://www.googleapis.com/oauth2/v2/userinfo";
    }
}
