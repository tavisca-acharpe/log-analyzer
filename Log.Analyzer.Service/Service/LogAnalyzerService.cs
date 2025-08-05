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
            emailBody = await GetCancellationStats(startDate, emailBody);
            emailBody = await GetExceptionAndFailures(applications, startDate, compareStartDate, emailBody);

            if (!string.IsNullOrWhiteSpace(emailBody))
            {
                await _notifier.SendNotification(ReportTranslator.ToHTMLReport(emailBody), toAddressesEmail);
            }
        }

        private async Task<string> GetBookingStats(DateTime startDate, string emailBody)
        {
            Console.WriteLine("Checking Latest Booking Logs");
            var latestBookings = await _elasticSearchService.GetDataAsync(_esSettings.BookingStatsQuery, startDate, DateTime.UtcNow);
            var existingBookings = await _elasticSearchService.GetDataAsync(_esSettings.BookingStatsQuery, startDate.AddDays(-1), DateTime.UtcNow.AddDays(-1));
            
            Console.WriteLine("Latest bookings count : " + latestBookings?.Count + " Yesterday bookings count : " + existingBookings?.Count);

            //Validate any 5 cids 
            if (latestBookings.Any())
            {
                var latestFive = latestBookings
                              .OrderBy(item => item.TimeStamp)
                              .Take(5)
                              .ToList();

                emailBody = string.Concat(emailBody, ReportTranslator.BookingHtmlTableStart("Booking", latestBookings?.Count ?? 0, existingBookings?.Count ?? 0));
                foreach (var booking in latestFive)
                {
                    var nsSorcQuery = string.Format(_esSettings.SorcBookingQuery, booking.Cid);
                    var ngSorc = await _elasticSearchService.GetDataAsync(nsSorcQuery, startDate, DateTime.UtcNow);

                    var travcomQuery = string.Format(_esSettings.TravcomBookingQuery, booking.Cid);
                    var travCom = await _elasticSearchService.GetDataAsync(travcomQuery, startDate, DateTime.UtcNow);

                    var dataMeshQuery = string.Format(_esSettings.DataMeshBookingQuery, booking.Cid);
                    var dataMesh = await _elasticSearchService.GetDataAsync(dataMeshQuery, startDate, DateTime.UtcNow);

                    emailBody = string.Concat(emailBody, ReportTranslator.BookingHtmlTableValues(booking.Cid, booking.Status, ngSorc?.FirstOrDefault()?.Status, travCom?.FirstOrDefault()?.Status, dataMesh?.FirstOrDefault()?.Status));
                }
                emailBody = string.Concat(emailBody, ReportTranslator.BookingHtmlTableEnd());
            }
            else
            {
                Console.WriteLine("No new bookings");
            }

            return emailBody;
        }

        private async Task<string> GetCancellationStats(DateTime startDate, string emailBody)
        {
            Console.WriteLine("\nChecking Latest cancellation Logs");
            var latestBookings = await _elasticSearchService.GetDataAsync(_esSettings.CancellationStatsQuery, startDate, DateTime.UtcNow);
            var existingBookings = await _elasticSearchService.GetDataAsync(_esSettings.CancellationStatsQuery, startDate.AddDays(-1), DateTime.UtcNow.AddDays(-1));

            Console.WriteLine("Latest cancellation count : " + latestBookings?.Count + " Yesterday cancellation count : " + existingBookings?.Count);

            //Validate any 5 cids 
            if (latestBookings.Any())
            {
                var latestFive = latestBookings
                              .OrderBy(item => item.TimeStamp)
                              .Take(5)
                              .ToList();

                emailBody = string.Concat(emailBody, ReportTranslator.BookingHtmlTableStart("Cancellation", latestBookings?.Count ?? 0, existingBookings?.Count ?? 0));
                foreach (var booking in latestFive)
                {
                    var nsSorcQuery = string.Format(_esSettings.SorcCancelQuery, booking.Cid);
                    var ngSorc = await _elasticSearchService.GetDataAsync(nsSorcQuery, startDate, DateTime.UtcNow);

                    var travcomQuery = string.Format(_esSettings.TravcomCancelQuery, booking.Cid);
                    var travCom = await _elasticSearchService.GetDataAsync(travcomQuery, startDate, DateTime.UtcNow);

                    var dataMeshQuery = string.Format(_esSettings.DataMeshCancelQuery, booking.Cid);
                    var dataMesh = await _elasticSearchService.GetDataAsync(dataMeshQuery, startDate, DateTime.UtcNow);

                    emailBody = string.Concat(emailBody, ReportTranslator.BookingHtmlTableValues(booking.Cid, booking.Status, ngSorc?.FirstOrDefault()?.Status, travCom?.FirstOrDefault()?.Status, dataMesh?.FirstOrDefault()?.Status));
                }
                emailBody = string.Concat(emailBody, ReportTranslator.BookingHtmlTableEnd());
            }
            else
            {
                Console.WriteLine("\nNo new cancellations");
            }

            return emailBody;
        }

        private async Task<string> GetExceptionAndFailures(List<string> applications, DateTime startDate, DateTime compareStartDate, string emailBody)
        {
            foreach (var application in applications)
            {
                Console.WriteLine(string.Format("\n******************** {0} *******************", application));
                Console.WriteLine("StartTime : " + startDate + " CompareTime : " + compareStartDate);

                var exceptionQuery = string.Format(_esSettings.ExceptionQuery, application);
                var apiFailuresQuery = string.Format(_esSettings.FailureQuery, application);

                var todayExceptionFailures = await _elasticSearchService.GetDataAsync(exceptionQuery, startDate, DateTime.UtcNow);
                var todayApiFailures = await _elasticSearchService.GetDataAsync(apiFailuresQuery, startDate, DateTime.UtcNow);

                Console.WriteLine("Todays exception count : " + todayExceptionFailures?.Count + " & failure count : " + todayApiFailures?.Count);

                var yesterdayExceptionFailures = await _elasticSearchService.GetDataAsync(exceptionQuery, compareStartDate, startDate);
                var yesterdayApiFailures = await _elasticSearchService.GetDataAsync(apiFailuresQuery, compareStartDate, startDate);

                Console.WriteLine("Yesterdays exception count : " + yesterdayExceptionFailures?.Count + " & failure count : " + yesterdayApiFailures?.Count);

                var uniqueExceptionFailures = todayExceptionFailures.Except(yesterdayExceptionFailures).ToList();
                var uniqueApiFailures = todayApiFailures.Except(yesterdayApiFailures).ToList();

                if (uniqueExceptionFailures.Any() || uniqueApiFailures.Any())
                {
                    string exceptionBody = GetExceptionFailures(todayExceptionFailures, yesterdayExceptionFailures, uniqueExceptionFailures);

                    string failureBody = GetApiFailures(todayApiFailures, yesterdayApiFailures, uniqueApiFailures);

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

            if (uniqueFailures.Any())
            {
                var failureMsg = "\nNew failures since yesterday";
                Console.WriteLine(failureMsg);
                failureBody = ReportTranslator.GenerateFailuresHtmlTable(uniqueFailures, todayFailures?.Count ?? 0, yesterdayFailures?.Count ?? 0);
                foreach (var failure in uniqueFailures)
                {
                    Console.WriteLine("cid: " + failure.Cid + " api: " + failure.Api + " verb: " + failure.Verb + " Msg:  " + failure.Msg);
                }
            }

            return failureBody;
        }

        private static string GetExceptionFailures(List<LogData>? todayFailures, List<LogData>? yesterdayFailures, List<LogData> uniqueFailures)
        {
            var exceptionBody = string.Empty;

            if (uniqueFailures.Any())
            {
                var exceptionMsg = "\nNew exceptions since yesterday";
                Console.WriteLine(exceptionMsg);
                exceptionBody = ReportTranslator.GenerateExceptionHtmlTable(uniqueFailures, todayFailures?.Count ?? 0, yesterdayFailures?.Count ?? 0);
                foreach (var failure in uniqueFailures)
                {
                    Console.WriteLine("cid: " + failure.Cid + " ex_type: " + failure.ExceptionType + " Msg:  " + failure.Msg);
                }
            }

            return exceptionBody;
        }
    }
}
