using Microsoft.Extensions.Logging;
using Poen.Models;
using Poen.Services.CoinMarketCap;
using RestSharp;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace Poen.Services.BlockChain
{
    public class BlockChainApi
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly ILogger<BlockChainApi> _logger;

        public BlockChainApi(HttpClient httpClient, string baseUrl, string apiKey, ILogger<BlockChainApi> logger)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
            _baseUrl = baseUrl;
            _logger = logger;
        }


        public async Task<TokenBalanceResponse?> GetBalance(Token token, string walletAddress)
        {
            var request = BuildRequest(query => {
                query["module"] = "account";
                query["action"] = "tokenbalance";
                query["contractaddress"] = token.Contract;
                query["address"] = walletAddress;
                query["tag"] = "latest";
            });

            if (token.Symbol == "BNB")
            {
                request = BuildRequest(query => {
                    query["module"] = "account";
                    query["action"] = "balance";
                    query["address"] = walletAddress;
                });
            }
            

            if (request == null)
                return null;

            return await ExecuteRequest<TokenBalanceResponse>(request);
        }

        private async Task<Response?> ExecuteRequest<Response>(HttpRequestMessage request)
        {
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return default;

            string responseBody = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            return JsonSerializer.Deserialize<Response>(responseBody, options);
        }
        HttpRequestMessage? BuildRequest(Action<System.Collections.Specialized.NameValueCollection> queryBuilder)
        {
            var URL = new UriBuilder(_baseUrl);
            var query = HttpUtility.ParseQueryString(string.Empty);
            queryBuilder(query);
            query["apikey"] = _apiKey;
            URL.Query = query.ToString();
            var request = new HttpRequestMessage(HttpMethod.Get, URL.ToString());
            return request;
        }

    }
}


