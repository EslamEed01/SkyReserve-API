namespace SkyReserve.Application.Settings
{
    public class SqsSettings
    {
        public const string SectionName = "AwsSqs";
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Region { get; set; } = "us-east-1";
        public string QueueUrl { get; set; } = string.Empty;
        public int MaxMessages { get; set; } = 10;
        public int WaitTimeSeconds { get; set; } = 20;
        public int VisibilityTimeoutSeconds { get; set; } = 30;
        public string DeadLetterQueueUrl { get; set; } = string.Empty;
        public int MaxReceiveCount { get; set; } = 3;
        public bool EnableDeadLetterQueue { get; set; } = true;
        public int MessageRetentionPeriod { get; set; } = 1209600;
    }
}