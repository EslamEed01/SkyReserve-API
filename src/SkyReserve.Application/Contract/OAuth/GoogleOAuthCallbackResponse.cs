using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyReserve.Application.Contract.OAuth
{
    public class GoogleOAuthCallbackResponse
    {
        public bool IsSuccess { get; set; }
        public string? JwtToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public GoogleUserInfo? UserInfo { get; set; }
        public string? Error { get; set; }
        public string? ErrorDescription { get; set; }
        public bool RequiresRegistration { get; set; }
    }

}
