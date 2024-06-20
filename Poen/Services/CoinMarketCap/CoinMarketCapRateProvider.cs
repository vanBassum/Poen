using Microsoft.Extensions.Options;
using Poen.Config;
using Poen.Models;
using Poen.Services.ConversionRates;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace Poen.Services.CoinMarketCap
{
    public class CoinMarketCapRateProvider : IConversionRateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly CoinMarketCapConfig _config;
        private DateTime lastFetched = DateTime.MinValue;
        public CoinMarketCapRateProvider(HttpClient httpClient, IOptions<CoinMarketCapConfig> config)
        {
            _httpClient = httpClient;
            _config = config.Value;
        }

        public async Task UpdateConversionRates(List<ConversionRate> rates)
        {
            if (DateTime.Now - lastFetched < TimeSpan.FromSeconds(_config.RateLimit))
                return;

            var response = await RequestData(rates);

            foreach (var rate in rates)
            {
                var coinData = (response?.Data?.FirstOrDefault(a => a.Key == rate.FromToken.Symbol))?.Value;
                var quote = (coinData?.Quote?.FirstOrDefault(a => a.Key == rate.ToToken.Symbol))?.Value;

                if (coinData == null)
                    continue;

                if (quote?.Price == null)
                    continue;

                rate.LastUpdated = DateTime.UtcNow;
                rate.Price = quote.Price;
            }

            lastFetched = DateTime.Now;
        }

        HttpRequestMessage? BuildRequest(List<ConversionRate> rates)
        {
            //var URL = new UriBuilder("https://sandbox-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest");
            var URL = new UriBuilder("https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest");

            if (rates == null || rates.Count == 0)
                return null;

            string from = string.Join(',', rates.Select(a => a.FromToken.Symbol).Distinct());
            string to = string.Join(',', rates.Select(a => a.ToToken.Symbol).Distinct());
            var queryBuilder = HttpUtility.ParseQueryString(string.Empty);
            //queryBuilder["start"] = "1";
            //queryBuilder["limit"] = "5000";
            queryBuilder["symbol"] = from;
            queryBuilder["convert"] = to;

            URL.Query = queryBuilder.ToString();
            var request = new HttpRequestMessage(HttpMethod.Get, URL.ToString());
            if (_config.ApiKey == null)
                throw new Exception("_config.ApiKey cant be null");

            request.Headers.Add("X-CMC_PRO_API_KEY", _config.ApiKey);

            return request;
        }



        async Task<CoinMarketCapResponse?> RequestData(List<ConversionRate> rates)
        {
            var request = BuildRequest(rates);
            if (request == null)
                return null;

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;

            string responseBody = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            return JsonSerializer.Deserialize<CoinMarketCapResponse>(responseBody, options);
        }
    }
}
