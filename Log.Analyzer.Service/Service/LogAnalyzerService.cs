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

        public async Task RunAnalysisAsync(List<string> applications)
        {
            foreach (var application in applications)
            {
                Console.WriteLine(string.Format("******************** {0} *******************", application));
                var todayFailures = await _elasticSearchService.GetDataAsync(application, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
                Console.WriteLine("Todays failure count {0} : " + todayFailures?.Count);
                var yesterdayFailures = await _elasticSearchService.GetDataAsync(application, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-2));
                Console.WriteLine("Yesterdays failure count {0} : " + yesterdayFailures?.Count);

                var newFailures = todayFailures.Except(yesterdayFailures).ToList();

                var uniqueFailures = newFailures
                                .GroupBy(f => f.Msg)
                                .Select(g => g.First())
                                .ToList();

                if (uniqueFailures.Any())
                {
                    Console.WriteLine("New failure/exceptions since yesterday:");
                    foreach (var failure in uniqueFailures)
                    {
                        Console.WriteLine("- " + failure.Msg);
                    }
                }
                else
                {
                    Console.WriteLine("No new failures found compared to yesterday.");
                }
            }
        }
    }
}
