namespace Log.Analyzer.Service.Contract
{
    public interface ILogAnalyzerService
    {
        Task RunAnalysisAsync(List<string> applications, DateTime startDate, DateTime endDate, List<string> toAddressesEmail, bool executeSorcReport);
    }
}
