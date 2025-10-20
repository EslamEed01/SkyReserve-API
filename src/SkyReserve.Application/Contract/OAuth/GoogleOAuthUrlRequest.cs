using System.ComponentModel.DataAnnotations;

namespace SkyReserve.Application.Contract.OAuth
{
    public class GoogleOAuthUrlRequest
    {
        public string? ReturnUrl { get; set; }
        public string[]? AdditionalScopes { get; set; }
    }

  
}