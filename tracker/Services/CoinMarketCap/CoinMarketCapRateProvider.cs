using Microsoft.Extensions.Options;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Tracker.Models;
using Tracker.Services.ConversionRates;

namespace Tracker.Services.CoinMarketCap
{
    public class CoinMarketCapRateProvider : IConversionRateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ApiKeys _apiKeys;

        public CoinMarketCapRateProvider(HttpClient httpClient, IOptions<ApiKeys> apiKeys)
        {
            _httpClient = httpClient;
            _apiKeys = apiKeys.Value;
        }

        public async Task UpdateConversionRates(List<ConversionRate> rates)
        {
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
            request.Headers.Add("X-CMC_PRO_API_KEY", _apiKeys.CoinMarketCap);

            return request;
        }



        async Task<CoinMarketCapResponse?> RequestData(List<ConversionRate> rates)
        {
            var request = BuildRequest(rates);
            if (request == null)
                return null;

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if(!response.IsSuccessStatusCode)
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
