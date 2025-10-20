namespace SkyReserve.Application.Settings
{
    public class ElasticsearchSettings
    {
        public const string SectionName = "Elasticsearch";

        public string Url { get; set; } = string.Empty;
        public string DefaultIndex { get; set; } = "flights";
    }
}
