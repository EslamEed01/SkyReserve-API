namespace SkyReserve.Application.DTOs.Payment.DTOs
{
    public class PaymentIntentResponse
    {
        public bool Success { get; set; }
        public string PaymentIntentId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}