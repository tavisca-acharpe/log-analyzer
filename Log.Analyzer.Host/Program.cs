using Log.Analyzer.Service;
using Log.Analyzer.Service.Contract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace MyConsoleApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register services here
                services.AddSingleton<ILogAnalyzerService, LogAnalyzerService>();
            })
            .Build();

        await RunLogAnalyzerTool(host.Services);
    }

    private static async Task RunLogAnalyzerTool(IServiceProvider services)
    {
        Stopwatch watch = Stopwatch.StartNew();

        try
        {
            System.Console.WriteLine("Log Analyzer Execution Started....");
            System.Console.WriteLine();

            var analyzerService = services.GetRequiredService<ILogAnalyzerService>();
            var analyzerRq = new List<string>() { "order_sync_webhook" };
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