namespace SkyReserve.Infrastructure.SMS
{
    public class TwilioSettings
    {
        public const string SectionName = "Twilio";

        public string AccountSid { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
        public string FromPhone { get; set; } = string.Empty;
    }
}