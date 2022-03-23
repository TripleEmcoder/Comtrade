using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Comtrade
{
    internal class ComtradeClient
    {
        private readonly HttpClient httpClient;
        private readonly IDistributedCache cache;
        private readonly JsonSerializerOptions options;

        public ComtradeClient(HttpClient httpClient, IDistributedCache cache)
        {
            this.httpClient = httpClient;
            this.cache = cache;

            options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
        }

        private async Task<T> GetFromCacheOrUpstream<T>(string uri, CancellationToken cancellationToken)
        {
            var cacheKey = uri + ".json";
            var json = await cache.GetStringAsync(cacheKey, cancellationToken);

            if (json == null)
                json = await httpClient.GetStringAsync(uri, cancellationToken);

            await cache.SetStringAsync(cacheKey, json, cancellationToken);
            return JsonSerializer.Deserialize<T>(json, options);
        }

        public async Task<ParameterResponse> Reporters(CancellationToken cancellationToken)
            => await GetFromCacheOrUpstream<ParameterResponse>("Data/cache/reporterAreas.json", cancellationToken);
        
        public async Task<ParameterResponse> Partners(CancellationToken cancellationToken)
            => await GetFromCacheOrUpstream<ParameterResponse>("Data/cache/partnerAreas.json", cancellationToken);

         public async Task<ParameterResponse> Flows(CancellationToken cancellationToken)
            => await GetFromCacheOrUpstream<ParameterResponse>("Data/cache/tradeRegimes.json", cancellationToken);

        public async Task<ParameterResponse> Commodities(string classification, CancellationToken cancellationToken)
            => await GetFromCacheOrUpstream<ParameterResponse>($"Data/cache/classification{classification}.json", cancellationToken);

        private static string GetDataUri(DataQuery query)
            => "api/get" + new QueryBuilder
            {
                { "type", "C" },
                { "freq", "A" },
                { "ps", "2020" },
                { "px", query.Classification },
                { "cc", query.Commodity },
                { "r", query.Reporter?.ToString() ?? "all" },
                { "p", query.Partner?.ToString() ?? "all"},
                { "rg", query.Flow?.ToString() ?? "all" },
            };

        public async Task<DataResponse?> Data(DataQuery query, CancellationToken cancellationToken)
        {
            var response = await GetFromCacheOrUpstream<DataResponse>(GetDataUri(query), cancellationToken);

            if (query.Partner == null)
                response.Results = response.Results.Where(r => r.PartnerCode != 0).ToList();

            return response;
        }
    }


    public enum ComtradeFlow
    {
        Import,
        Export,
    }
}