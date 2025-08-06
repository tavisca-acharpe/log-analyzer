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
            emailBody = await GetNgSorcFailureStats(startDate, emailBody, toAddressesEmail);
            emailBody = await GetExceptionAndFailures(applications, startDate, compareStartDate, emailBody);

            if (!string.IsNullOrWhiteSpace(emailBody))
            {
                await _notifier.SendNotification(ReportTranslator.ToHTMLReport(emailBody), toAddressesEmail);
            }
        }

        private async Task<string> GetBookingStats(DateTime startDate, string emailBody)
        {
            Console.WriteLine("\n**********************************************");
            Console.WriteLine("Checking Latest Booking Logs");

            var todayEndTime = DateTime.UtcNow;
            Console.WriteLine("Todays StartTime : " + startDate + " Todays EndTime : " + todayEndTime);
            var latestBookings = await _elasticSearchService.GetDataAsync(_esSettings.BookingStatsQuery, startDate, todayEndTime);

            var yesterdayStartTime = startDate.AddDays(-1);
            var yesterdayEndTime = DateTime.UtcNow.AddDays(-1);
            Console.WriteLine("Yesterdays StartTime : " + yesterdayStartTime + " Yesterdays EndTime : " + yesterdayEndTime);
            var existingBookings = await _elasticSearchService.GetDataAsync(_esSettings.BookingStatsQuery, yesterdayStartTime, yesterdayEndTime);

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
            Console.WriteLine("\n**********************************************");
            Console.WriteLine("Checking Latest cancellation Logs");

            var todayEndTime = DateTime.UtcNow;
            Console.WriteLine("Todays StartTime : " + startDate + " Todays EndTime : " + todayEndTime);
            var latestBookings = await _elasticSearchService.GetDataAsync(_esSettings.CancellationStatsQuery, startDate, todayEndTime);

            var yesterdayStartTime = startDate.AddDays(-1);
            var yesterdayEndTime = DateTime.UtcNow.AddDays(-1);
            Console.WriteLine("Yesterdays StartTime : " + yesterdayStartTime + " Yesterdays EndTime : " + yesterdayEndTime);
            var existingBookings = await _elasticSearchService.GetDataAsync(_esSettings.CancellationStatsQuery, yesterdayStartTime, yesterdayEndTime);

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

        private async Task<string> GetNgSorcFailureStats(DateTime startDate, string emailBody, List<string> toAddressesEmail)
        {
            if (!toAddressesEmail.Contains("sorc"))
            {
                Console.WriteLine("SORC Report Disabled.");
                return emailBody; // Exit the method
            }

            Console.WriteLine("\n**********************************************");
            Console.WriteLine("\nSORC Create Order Checking Last 12 hrs Bookings");

            var startTime = DateTime.UtcNow.AddHours(-12);
            var endTime = DateTime.UtcNow;
            Console.WriteLine("Booking Logs StartTime : " + startDate + " EndTime : " + endTime);
            var latestBookings = await _elasticSearchService.GetDataAsync(_esSettings.BookingSuccessStatsQuery, startDate, endTime);
            Console.WriteLine("Total Booking Count : " + latestBookings?.Count);

            Console.WriteLine("Sorc Logs StartTime : " + startDate + " EndTime : " + endTime.AddMinutes(5));
            var ngSorcCreateOrder = await _elasticSearchService.GetDataAsync(_esSettings.NgSorcCreateOrder, startDate, endTime.AddMinutes(5));
            Console.WriteLine("NgSorc create order count : " + ngSorcCreateOrder?.Count);

            var missingOrders = ngSorcCreateOrder?.Where(o2 => !latestBookings.Any(o1 => o1.SuperPNR == o2.SuperPNR))?.ToList();

            if (missingOrders.Any())
            {
                emailBody = string.Concat(emailBody, ReportTranslator.SorcCreateOrderDiffernceTable(missingOrders?.Count ?? 0));
                foreach (var booking in missingOrders)
                {
                    emailBody = string.Concat(emailBody, ReportTranslator.SorcCreateOrderTableValues(booking.Cid, booking.SuperPNR, booking.OrderId));
                }
                emailBody = string.Concat(emailBody, ReportTranslator.BookingHtmlTableEnd());
            }
            else
            {
                Console.WriteLine("No Differnce Found");
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

            var uniqueFailuresGrouped = uniqueFailures
                                       .GroupBy(f => f.Msg)?
                                       .Select(g => g.First())
                                       .ToList();

            if (uniqueFailuresGrouped.Any())
            {
                var failureMsg = "\nNew failures since yesterday";
                Console.WriteLine(failureMsg);
                failureBody = ReportTranslator.GenerateFailuresHtmlTable(uniqueFailuresGrouped, todayFailures?.Count ?? 0, yesterdayFailures?.Count ?? 0);
                foreach (var failure in uniqueFailuresGrouped)
                {
                    Console.WriteLine("cid: " + failure.Cid + " api: " + failure.Api + " verb: " + failure.Verb + " Msg:  " + failure.Msg);
                }
            }

            return failureBody;
        }

        private static string GetExceptionFailures(List<LogData>? todayFailures, List<LogData>? yesterdayFailures, List<LogData> uniqueFailures)
        {
            var exceptionBody = string.Empty;

            var uniqueFailuresGrouped = uniqueFailures
                                       .GroupBy(f => f.Msg)?
                                       .Select(g => g.First())
                                       .ToList();

            if (uniqueFailuresGrouped.Any())
            {
                var exceptionMsg = "\nNew exceptions since yesterday";
                Console.WriteLine(exceptionMsg);
                exceptionBody = ReportTranslator.GenerateExceptionHtmlTable(uniqueFailuresGrouped, todayFailures?.Count ?? 0, yesterdayFailures?.Count ?? 0);
                foreach (var failure in uniqueFailuresGrouped)
                {
                    Console.WriteLine("cid: " + failure.Cid + " ex_type: " + failure.ExceptionType + " Msg:  " + failure.Msg);
                }
            }

            return exceptionBody;
        }
    }
}