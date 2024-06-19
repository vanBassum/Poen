using Microsoft.Extensions.Options;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Tracker.Models;
using Tracker.Services.ConversionRates;

namespace Tracker.Services.CoinMarketCap
{
    public class CoinMarketCapConversionProvider : IConversionRateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ApiKeys _apiKeys;

        public CoinMarketCapConversionProvider(HttpClient httpClient, IOptions<ApiKeys> apiKeys)
        {
            _httpClient = httpClient;
            _apiKeys = apiKeys.Value;
        }


        public async Task<ConversionRate?> GetConversionRateAsync(Token fromToken, Token toToken)
        {
            var response = await RequestData(fromToken, toToken);
            var coinData = (response?.Data?.FirstOrDefault(a => a.Key == fromToken.Symbol))?.Value;
            var quote = (coinData?.Quote?.FirstOrDefault(a => a.Key == toToken.Symbol))?.Value;

            if (coinData == null)
                return null;

            if (quote?.Price == null)
                return null;

            return new ConversionRate
            {
                FromToken = fromToken,
                ToToken = toToken,
                LastUpdated = coinData.LastUpdated,
                Rate = quote.Price.Value,
            };
        }


        async Task<CoinMarketCapResponse?> RequestData(Token fromToken, Token toToken)
        {
            //var URL = new UriBuilder("https://sandbox-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest");
            var URL = new UriBuilder("https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest");

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            //queryString["start"] = "1";
            //queryString["limit"] = "5000";
            queryString["convert"] = toToken.Symbol;
            queryString["symbol"] = fromToken.Symbol;


            URL.Query = queryString.ToString();


            var request = new HttpRequestMessage(HttpMethod.Get, URL.ToString());
            request.Headers.Add("X-CMC_PRO_API_KEY", _apiKeys.CoinMarketCap);


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
