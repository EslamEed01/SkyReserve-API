namespace SkyReserve.Application.DTOs.Payment.DTOs
{
    public class PaymentIntent
    {
        public string Id { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}