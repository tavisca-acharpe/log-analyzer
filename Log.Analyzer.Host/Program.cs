using Log.Analyzer.ElasticSearch;
using Log.Analyzer.Service;
using Log.Analyzer.Service.Contract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace MyConsoleApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        var environment = args.FirstOrDefault() ?? "qa";
        Console.WriteLine($"Running environment: {environment}");

        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.SetBasePath(Directory.GetCurrentDirectory());
                configBuilder.AddJsonFile($"appsettings.{environment}.json", optional: false);
            })
            .ConfigureServices((context, services) =>
            {
                // Register services here
                services.AddSingleton<ILogAnalyzerService, LogAnalyzerService>();
                services.AddSingleton<IElasticSearchService, ElasticSearchService>();
            })
            .Build();

        await RunLogAnalyzerTool(host.Services, args);
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

            if (args.Length >= 2)
            {
                string appsArg = args[1];
                string[] applications = appsArg.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                Console.WriteLine("Applications to run :");
                foreach (var app in applications)
                {
                    Console.WriteLine($"- {app}");
                }
                analyzerRq = applications.ToList();
            }

            await analyzerService.RunAnalysisAsync(analyzerRq);
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
}