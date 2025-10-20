namespace SkyReserve.Infrastructure.Email
{
    public class SESsettings
    {
        public const string SectionName = "AwsSes";

        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Region { get; set; } = "us-east-1";
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public string? ReplyToEmail { get; set; }
        public string? ConfigurationSet { get; set; }
        public Dictionary<string, string>? MessageTags { get; set; }
        public bool UseSandbox { get; set; } = false;
    }
}