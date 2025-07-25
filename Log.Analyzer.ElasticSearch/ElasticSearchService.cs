using Nest;
using System.Collections.Concurrent;

namespace Log.Analyzer.ElasticSearch
{
    public class ElasticSearchService : IElasticSearchService
    {
        public async Task<List<LogData>> GetDataAsync(DateTime startDate, DateTime endDate)
        {
            var queryString = "app_name : order_sync_webhook AND type : exception";
            var queryStrings = new List<string>();

            do
            {
                var esQuery = String.Format(queryString, startDate.ToString("yyyy-MM-ddTHH:mm:ssZ"), startDate.AddHours(6).ToString("yyyy-MM-ddTHH:mm:ssZ"));
                queryStrings.Add(esQuery);
                startDate = startDate.AddHours(6);
            } while (startDate <= endDate);

            var data = new ConcurrentBag<List<LogData>>();

            await Task.WhenAll(
            queryStrings.Select(async query =>
            {
                var result = await GetData(query);
                data.Add(result);
            }));

            var response = data.SelectMany(r => r).ToList();
            return response;
        }

        private async Task<List<LogData>> GetData(string queryString)
        {
            var responses = new List<LogData>();
            bool moreResults = true;
            int count = 0;

            var client = new ElasticClient(new ConnectionSettings(new Uri("https://es.qa.cnxloyalty.com")).DefaultIndex("logs-*").DisableDirectStreaming());

            while (moreResults)
            {
                var searchRequest = new SearchRequest
                {
                    From = count,
                    Size = 10000,
                    QueryOnQueryString = queryString
                };

                ISearchResponse<LogData> response = client.Search<LogData>(searchRequest);
                Console.WriteLine("Total : " + response.Total);
                Console.WriteLine("IsValid : " + response.IsValid);
                Console.WriteLine("Documents : " + response.Documents);
                Console.WriteLine("Documents : " + response);
                
                if (response.Documents.Count == 0)
                {
                    break;
                }

                foreach (var data in response.Documents)
                {
                    responses.Add(data);
                }

                count += 10000;
            }
            return responses;
        }
    }
}
