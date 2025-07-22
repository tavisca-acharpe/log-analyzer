using Log.Analyzer.ElasticSearch;
using Log.Analyzer.Service.Contract;

namespace Log.Analyzer.Service
{
    public class LogAnalyzerService : ILogAnalyzerService
    {
        private readonly IElasticSearchService _elasticSearchService;
        public LogAnalyzerService()
        {
            _elasticSearchService = new ElasticSearchService();
        }

        public async Task<List<string>> RunAnalysisAsync(List<string> products)
        {
            var todayFailures = await _elasticSearchService.GetDataAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
            var yesterdayFailures = await _elasticSearchService.GetDataAsync(DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-2));

            var newFailures = todayFailures.Except(yesterdayFailures).ToList();

            if (newFailures.Any())
            {
                Console.WriteLine("New failure/exceptions since yesterday:");
                foreach (var failure in newFailures)
                {
                    Console.WriteLine("- " + failure);
                }
            }
            else
            {
                Console.WriteLine("No new failures found compared to yesterday.");
            }

            return null;
        }
    }
}
