using Log.Analyzer.ElasticSearch;
using Log.Analyzer.EmailAdapter;
using Log.Analyzer.Service;
using Log.Analyzer.Service.Contract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System;

namespace Log.Analyzer.Host;

public class Program
{
    public static async Task Main(string[] args)
    {
        string[] inputs = args;
        if (args.Length > 0)
        {
            inputs = args[0].Split(',');
        }
 
        var environment = inputs.FirstOrDefault() ?? "qa";
        Console.WriteLine($"Running environment: {environment}");

        using IHost host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(inputs)
            .ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
                configBuilder.AddJsonFile($"appsettings.{environment}.json", optional: false);
            })
            .ConfigureServices((context, services) =>
            {
                // Register services here
                services.AddSingleton<ILogAnalyzerService, LogAnalyzerService>();
                services.AddSingleton<IElasticSearchService, ElasticSearchService>();
                services.AddSingleton<INotifier, EmailNotifier>();
            })
            .Build();

        await RunLogAnalyzerTool(host.Services, inputs);
    }

    private static async Task RunLogAnalyzerTool(IServiceProvider services, string[] args)
    {
        Stopwatch watch = Stopwatch.StartNew();

        try
        {
            System.Console.WriteLine("Log Analyzer Execution Started....");
            System.Console.WriteLine();

            var analyzerService = services.GetRequiredService<ILogAnalyzerService>();
            var analyzerRq = new List<string>() { "order_sync_webhook"};
            DateTime startDate = DateTime.UtcNow.AddDays(-1);
            DateTime compareStartDate = DateTime.UtcNow.AddDays(-2);
            var toAddressEmail = new List<string>() { "acharpe@tavisca.com" };

            ReadInputParameters(args, ref analyzerRq, ref startDate, ref compareStartDate, ref toAddressEmail);

            await analyzerService.RunAnalysisAsync(analyzerRq, startDate, compareStartDate, toAddressEmail);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Exception occured");
            System.Console.WriteLine($"Exception msg : " + ex.Message);
            System.Console.WriteLine($"Exception : " + ex);
        }
        finally
        {
            System.Console.WriteLine();
            System.Console.WriteLine($"Log Analyzer Execution Done....Total Time taken {watch.ElapsedMilliseconds}");
        }
    }

    private static void ReadInputParameters(string[] args, ref List<string> analyzerRq, ref DateTime startDate, ref DateTime compareStartDate, ref List<string> toAddressEmail)
    {
        if (args.Length >= 2)
        {
            string appsArg = args[1];
            string[] applications = appsArg.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (applications.Length > 0)
            {
                Console.WriteLine("Applications to run :");
                foreach (var app in applications)
                {
                    Console.WriteLine($"- {app}");
                }
                analyzerRq = applications.ToList();
            }
        }

        if (args.Length >= 3)
        {
            string appsArg = args[2];
            string[] emails = appsArg.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (emails.Length > 0)
            {
                Console.WriteLine("Emails to send :");
                foreach (var email in emails)
                {
                    Console.WriteLine($"- {email}");
                }
                toAddressEmail = emails.ToList();
            }
        }

        if (args.Length >= 4)
        {
            string inputDateTime = args[3];
            if (DateTime.TryParseExact(inputDateTime, "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime parsedDateTime))
            {
                startDate = parsedDateTime;
            }
            else
            {
                Console.WriteLine("Invalid start date time format. Default DataTime : " + startDate);
            }
        }

        if (args.Length >= 5)
        {
            string inputDateTime = args[4];
            if (DateTime.TryParseExact(inputDateTime, "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime parsedDateTime))
            {
                compareStartDate = parsedDateTime;
            }
            else
            {
                Console.WriteLine("Invalid compare date time format. Default DataTime : " + compareStartDate);
            }
        }
    }
}