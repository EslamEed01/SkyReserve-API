namespace SkyReserve.Application.Settings
{
    public class RedisSettings
    {
        public const string SectionName = "Redis";

        public string ConnectionString { get; set; } = "localhost:6379";
        public string KeyPrefix { get; set; } = "SkyReserve:";
        public int Database { get; set; } = 0;
        public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromHours(1);
    }
}