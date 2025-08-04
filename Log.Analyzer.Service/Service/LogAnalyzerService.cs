using Log.Analyzer.ElasticSearch;
using Log.Analyzer.EmailAdapter;
using Log.Analyzer.Service.Contract;
using Log.Analyzer.Service.Translators;

namespace Log.Analyzer.Service
{
    public class LogAnalyzerService : ILogAnalyzerService
    {
        private readonly IElasticSearchService _elasticSearchService;
        private readonly INotifier _notifier;
        public LogAnalyzerService(IElasticSearchService elasticSearchService, INotifier notifier)
        {
            _elasticSearchService = elasticSearchService;
            _notifier = notifier;
        }

        public async Task RunAnalysisAsync(List<string> applications, DateTime startDate, DateTime compareStartDate, List<string> toAddressesEmail)
        {
            var emailBody = string.Empty;
            foreach (var application in applications)
            {
                Console.WriteLine(string.Format("******************** {0} *******************", application));
                var todayFailures = await _elasticSearchService.GetDataAsync(application, startDate, DateTime.UtcNow);
                Console.WriteLine("Todays exception count : " + todayFailures?.Where(f => f.Type == "exception")?.Count() + " & failure count : " + todayFailures?.Where(f => f.Type == "api")?.Count());
                var yesterdayFailures = await _elasticSearchService.GetDataAsync(application, compareStartDate, startDate);
                Console.WriteLine("Yesterdays exception count : " + yesterdayFailures?.Where(f => f.Type == "exception")?.Count() + " & failure count : " + yesterdayFailures?.Where(f => f.Type == "api")?.Count());

                var uniqueFailures = todayFailures.Except(yesterdayFailures).ToList();

                if (uniqueFailures.Any())
                {
                    string exceptionBody = GetExceptionFailures(todayFailures, yesterdayFailures, uniqueFailures);

                    string failureBody = GetApiFailures(todayFailures, yesterdayFailures, uniqueFailures);

                    emailBody = string.Concat(emailBody, ReportTranslator.GenerateApplicationHtmlTable(application, exceptionBody, failureBody));
                }
                else
                {
                    Console.WriteLine("No new failures found compared to yesterday.");
                }
            }

            if (!string.IsNullOrWhiteSpace(emailBody))
            {
                await _notifier.SendNotification(ReportTranslator.ToHTMLReport(emailBody), toAddressesEmail);
            }
        }

        private static string GetApiFailures(List<LogData> todayFailures, List<LogData> yesterdayFailures, List<LogData> uniqueFailures)
        {
            var failureBody = string.Empty;
            var apiFailures = uniqueFailures
                               .Where(x => x.Type == "api")?
                               .GroupBy(f => f.Verb)?
                               .Select(g => g.First())
                               .ToList();

            if (apiFailures.Any())
            {
                var failureMsg = "New failures since yesterday";
                Console.WriteLine(failureMsg);
                failureBody = ReportTranslator.GenerateFailuresHtmlTable(apiFailures, todayFailures?.Where(f => f.Type == "api")?.Count(), yesterdayFailures?.Where(f => f.Type == "api")?.Count());
                foreach (var failure in apiFailures)
                {
                    Console.WriteLine("cid: " + failure.Cid + " api: " + failure.Api + " verb: " + failure.Verb + " Msg:  " + failure.Msg);
                }
            }

            return failureBody;
        }

        private static string GetExceptionFailures(List<LogData>? todayFailures, List<LogData>? yesterdayFailures, List<LogData> uniqueFailures)
        {
            var exceptionBody = string.Empty;
            var exceptionFailures = uniqueFailures
                                    .Where(x => x.Type == "exception")?
                                    .GroupBy(f => f.Msg)?
                                    .Select(g => g.First())
                                    .ToList();

            if (exceptionFailures.Any())
            {
                var exceptionMsg = "New exceptions since yesterday";
                Console.WriteLine(exceptionMsg);
                exceptionBody = ReportTranslator.GenerateExceptionHtmlTable(exceptionFailures, todayFailures?.Where(f => f.Type == "exception")?.Count(), yesterdayFailures?.Where(f => f.Type == "exception")?.Count());
                foreach (var failure in exceptionFailures)
                {
                    Console.WriteLine("cid: " + failure.Cid + " ex_type: " + failure.ExceptionType + " Msg:  " + failure.Msg);
                }
            }

            return exceptionBody;
        }
    }
}
