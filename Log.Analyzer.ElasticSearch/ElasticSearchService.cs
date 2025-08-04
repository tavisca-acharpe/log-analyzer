using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using Nest;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

namespace Log.Analyzer.ElasticSearch
{
    public class ElasticSearchService : IElasticSearchService
    {
        private readonly ESConfigurations _esSettings;
        public ElasticSearchService(IConfiguration configuration)
        {
            _esSettings = configuration.GetSection("ElasticSearch").Get<ESConfigurations>();
        }

        public async Task<List<LogData>> GetDataAsync(string query, DateTime startDate, DateTime endDate)
        {
            var queryStrings = new List<string>();
            do
            {
                var exQuery = String.Format(query, startDate.ToString("yyyy-MM-ddTHH:mm:ssZ"), startDate.AddHours(_esSettings.SplitQueryByHours).ToString("yyyy-MM-ddTHH:mm:ssZ"));
                queryStrings.Add(exQuery);
                startDate = startDate.AddHours(_esSettings.SplitQueryByHours);
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

            var client = new ElasticClient(new ConnectionSettings(new SingleNodeConnectionPool(new Uri(_esSettings.Url)), new AwsHttpConnection(_esSettings.Region)).DefaultIndex(_esSettings.DefaultIndex).DisableDirectStreaming());

            while (moreResults)
            {
                var searchRequest = new SearchRequest
                {
                    From = count,
                    Size = _esSettings.BatchSize,
                    Query = new QueryStringQuery
                    {
                        DefaultField = "FIELD",
                        Query = queryString
                    }
                };

                ISearchResponse<LogData> response = client.Search<LogData>(searchRequest);

                if (response.Documents.Count == 0)
                {
                    break;
                }

                foreach (var data in response.Documents)
                {
                    responses.Add(data);
                }

                count += _esSettings.BatchSize;
            }
            return responses;
        }
    }
}
