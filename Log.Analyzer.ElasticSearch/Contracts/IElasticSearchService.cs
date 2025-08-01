namespace Log.Analyzer.ElasticSearch
{
    public interface IElasticSearchService
    {
        Task<List<LogData>> GetDataAsync(string application, DateTime startDate, DateTime endDate);
    }
}
