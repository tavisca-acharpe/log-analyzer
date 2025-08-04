using Log.Analyzer.ElasticSearch;
using Log.Analyzer.EmailAdapter;
using Log.Analyzer.Service.Contract;
using Log.Analyzer.Service.Translators;
using Microsoft.Extensions.Configuration;

namespace Log.Analyzer.Service
{
    public class LogAnalyzerService : ILogAnalyzerService
    {
        private readonly IElasticSearchService _elasticSearchService;
        private readonly INotifier _notifier;
        private readonly ESConfigurations _esSettings;

        public LogAnalyzerService(IConfiguration configuration, IElasticSearchService elasticSearchService, INotifier notifier)
        {
            _esSettings = configuration.GetSection("ElasticSearch").Get<ESConfigurations>();
            _elasticSearchService = elasticSearchService;
            _notifier = notifier;
        }

        public async Task RunAnalysisAsync(List<string> applications, DateTime startDate, DateTime compareStartDate, List<string> toAddressesEmail)
        {
            var emailBody = string.Empty;

            emailBody = await GetBookingStats(startDate, emailBody);
            emailBody = await GetExceptionAndFailures(applications, startDate, compareStartDate, emailBody);

            if (!string.IsNullOrWhiteSpace(emailBody))
            {
                await _notifier.SendNotification(ReportTranslator.ToHTMLReport(emailBody), toAddressesEmail);
            }
        }

        private async Task<string> GetBookingStats(DateTime startDate, string emailBody)
        {
            Console.WriteLine("Checking Latest Booking Logs");
            var bookings = await _elasticSearchService.GetDataAsync(_esSettings.BookingStatsQuery, startDate, DateTime.UtcNow);
            Console.WriteLine("Todays bookings count : " + bookings?.Count);

            //Validate any 5 cids 
            if (bookings.Any())
            {
                var latestFive = bookings
                              .OrderBy(item => item.TimeStamp)
                              .Take(5)
                              .ToList();

                emailBody = string.Concat(emailBody, ReportTranslator.BookingHtmlTableStart());
                foreach (var booking in latestFive)
                {
                    var nsSorcQuery = string.Format(_esSettings.SorcBookingQuery, booking.Cid);
                    var ngSorc = await _elasticSearchService.GetDataAsync(nsSorcQuery, startDate, DateTime.UtcNow);

                    var travcomQuery = string.Format(_esSettings.SorcBookingQuery, booking.Cid);
                    var travCom = await _elasticSearchService.GetDataAsync(travcomQuery, startDate, DateTime.UtcNow);

                    var dataMeshQuery = string.Format(_esSettings.DataMeshBookingQuery, booking.Cid);
                    var dataMesh = await _elasticSearchService.GetDataAsync(travcomQuery, startDate, DateTime.UtcNow);

                    emailBody = string.Concat(emailBody, ReportTranslator.BookingHtmlTableValues(booking.Cid, booking.Status, ngSorc?.FirstOrDefault().Status, travCom?.FirstOrDefault().Status, dataMesh?.FirstOrDefault().Status));
                }
                emailBody = string.Concat(emailBody, ReportTranslator.BookingHtmlTableEnd());
            }
            else
            {
                Console.WriteLine("No new bookings");
            }

            return emailBody;
        }

        private async Task<string> GetExceptionAndFailures(List<string> applications, DateTime startDate, DateTime compareStartDate, string emailBody)
        {
            foreach (var application in applications)
            {
                Console.WriteLine(string.Format("******************** {0} *******************", application));

                var exceptionQuery = string.Format(_esSettings.ExceptionQuery, application);
                var apiFailuresQuery = string.Format(_esSettings.FailureQuery, application);

                var todayExceptionFailures = await _elasticSearchService.GetDataAsync(exceptionQuery, startDate, DateTime.UtcNow);
                var todayApiFailures = await _elasticSearchService.GetDataAsync(apiFailuresQuery, startDate, DateTime.UtcNow);

                var todayFailures = todayExceptionFailures;
                todayFailures.AddRange(todayApiFailures);
                Console.WriteLine("Todays exception count : " + todayExceptionFailures?.Count + " & failure count : " + todayApiFailures?.Count);

                var yesterdayExceptionFailures = await _elasticSearchService.GetDataAsync(exceptionQuery, compareStartDate, startDate);
                var yesterdayApiFailures = await _elasticSearchService.GetDataAsync(apiFailuresQuery, compareStartDate, startDate);

                var yesterdayFailures = yesterdayExceptionFailures;
                yesterdayFailures.AddRange(yesterdayApiFailures);
                Console.WriteLine("Yesterdays exception count : " + yesterdayExceptionFailures?.Count + " & failure count : " + yesterdayApiFailures?.Count);

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

            return emailBody;
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
