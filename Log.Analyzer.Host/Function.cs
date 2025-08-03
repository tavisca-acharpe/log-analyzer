using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Log.Analyzer.Host
{
#pragma warning disable CA1716 // Identifiers should not match keywords
    public class Function
#pragma warning restore CA1716 // Identifiers should not match keywords
    {
        public async Task HandleAsync(Routine routine)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            environment = environment ?? "qa";

            var task = Program.Main(new string[] { environment });
            task.Wait();
        }
    }

    public class Routine
    {
        public string Environment { get; set; }
    }
}
