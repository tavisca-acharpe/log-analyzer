using Log.Analyzer.ElasticSearch;
using Log.Analyzer.Service.Contract;

namespace Log.Analyzer.Service
{
    public class LogAnalyzerService : ILogAnalyzerService
    {
        private readonly IElasticSearchService _elasticSearchService;
        public LogAnalyzerService(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        public async Task RunAnalysisAsync(List<string> applications)
        {
            foreach (var application in applications)
            {
                Console.WriteLine(string.Format("******************** {0} *******************", application));
                var todayFailures = await _elasticSearchService.GetDataAsync(application, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
                Console.WriteLine("Todays exception & failure count : " + todayFailures?.Count);
                var yesterdayFailures = await _elasticSearchService.GetDataAsync(application, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));
                Console.WriteLine("Yesterdays exception & failure count : " + yesterdayFailures?.Count);

                var uniqueFailures = todayFailures.Except(yesterdayFailures).ToList();

                if (uniqueFailures.Any())
                {
                    var exceptionFailures = uniqueFailures
                                            .Where(x => x.Type == "exception")?
                                            .GroupBy(f => f.Msg)?
                                            .Select(g => g.First())
                                            .ToList();

                    if (exceptionFailures.Any())
                    {
                        Console.WriteLine("New exceptions since yesterday:");
                        foreach (var failure in exceptionFailures)
                        {
                            Console.WriteLine("cid: " + failure.Cid + " ex_type: " + failure.ExceptionType + " Msg:  " + failure.Msg);
                        }
                    }

                    var apiFailures = uniqueFailures
                                       .Where(x => x.Type == "api")?
                                       .GroupBy(f => f.Verb)?
                                       .Select(g => g.First())
                                       .ToList();

                    if (apiFailures.Any())
                    {
                        Console.WriteLine("New failures since yesterday:");
                        foreach (var failure in apiFailures)
                        {
                            Console.WriteLine("cid: " + failure.Cid + " api: " + failure.Api + " verb: " + failure.Verb + " Msg:  " + failure.Msg);
                        }
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
