namespace Log.Analyzer.ElasticSearch
{
    public interface IElasticSearchService
    {
        Task<List<LogData>> GetDataAsync(string query, DateTime startDate, DateTime endDate);
    }
}
