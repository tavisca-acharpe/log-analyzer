namespace Log.Analyzer.ElasticSearch
{
    public class ESConfigurations
    {
        public string Url { get; set; }
        public string Region { get; set; }
        public string DefaultIndex { get; set; }
        public int TimePeriodInDays { get; set; }
        public int TimePeriodInHours { get; set; }
        public int TimePeriodInMinutes { get; set; }
        public int BatchSize { get; set; }
    }
}
