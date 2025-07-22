namespace Log.Analyzer.Service.Contract
{
    public interface ILogAnalyzerService
    {
        Task<List<string>> RunAnalysisAsync(List<string> applications);
    }
}
