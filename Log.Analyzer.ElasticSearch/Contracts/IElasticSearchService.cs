namespace Log.Analyzer.ElasticSearch
{
    public interface IElasticSearchService
    {
        Task<List<string>> GetDataAsync(DateTime startDate, DateTime endDate);
    }
}
