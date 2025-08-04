namespace Log.Analyzer.ElasticSearch
{
    public class ESConfigurations
    {
        public string Url { get; set; }
        public string Region { get; set; }
        public string DefaultIndex { get; set; }
        public int BatchSize { get; set; }
        public int SplitQueryByHours { get; set; }
        public string ExceptionQuery { get; set; }
        public string FailureQuery { get; set; }
        public string BookingStatsQuery { get; set; }
        public string SorcBookingQuery { get; set; }
        public string DataMeshBookingQuery { get; set; }
        public string TravcomBookingQuery { get; set; }
        public string CancellationStatsQuery { get; set; }
        public string SorcCancelQuery { get; set; }
        public string TravcomCancelQuery { get; set; }
        public string DataMeshCancelQuery { get; set; }
    }
}