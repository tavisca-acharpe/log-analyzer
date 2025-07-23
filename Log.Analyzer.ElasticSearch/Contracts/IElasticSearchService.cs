namespace Log.Analyzer.ElasticSearch
{
    public interface IElasticSearchService
    {
        Task<List<LogData>> GetDataAsync(DateTime startDate, DateTime endDate);
    }
}
