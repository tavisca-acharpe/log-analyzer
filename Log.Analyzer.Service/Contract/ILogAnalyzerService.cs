namespace Log.Analyzer.Service.Contract
{
    public interface ILogAnalyzerService
    {
        Task RunAnalysisAsync(List<string> applications);
    }
}
