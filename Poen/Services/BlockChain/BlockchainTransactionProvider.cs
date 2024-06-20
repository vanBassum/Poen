using Poen.Models;
using Poen.Services.Transactions;
using System.Text.Json;

namespace Poen.Services.BlockChain
{
    public class BlockchainTransactionProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _apiKey;

        public BlockchainTransactionProvider(HttpClient httpClient, string baseUrl, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
            _baseUrl = baseUrl;
        }

        public async Task<List<Transaction>> GetTransactionsAsync(string walletAddress)
        {
            var result = await RequestTransactions(walletAddress);
            if (result == null)
                return new List<Transaction>();

            return result.result.Select(tx => ConvertTransaction(tx)).ToList();
        }

        private async Task<BlockChainTransactionResponse?> RequestTransactions(string walletAddress)
        {
            string apiUrl = GetApiUrl(walletAddress);
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<BlockChainTransactionResponse>(responseBody);
            return responseObject;
        }

        private string GetApiUrl(string walletAddress)
        {
            return $"{_baseUrl}?module=account&action=tokentx&address={walletAddress}&startblock=0&endblock=99999999&sort=asc&apikey={_apiKey}";
        }


        Transaction ConvertTransaction(BlockChainTransaction tx)
        {
            decimal val = ToDecimal(tx.value, 0);
            long dec = ToLong(tx.tokenDecimal, 0);
            decimal value = val / (decimal)Math.Pow(10, dec);

            return new Transaction
            {
                From = tx.from ?? "",
                To = tx.to ?? "",
                TimeStamp = ToDateTime(tx.timeStamp, DateTime.MinValue),
                Token = new Token
                {
                    Contract = tx.contractAddress ?? "",
                    Name = tx.tokenName ?? "",
                    Symbol = tx.tokenSymbol ?? "",
                },
                Value = value,
            };
        }

        public static decimal ToDecimal(string? input, decimal defaultValue = 0)
        {
            return decimal.TryParse(input, out var result) ? result : defaultValue;
        }

        public static long ToLong(string? input, long defaultValue = 0)
        {
            return long.TryParse(input, out var result) ? result : defaultValue;
        }

        public static DateTime ToDateTime(string? input, DateTime defaultValue)
        {
            return long.TryParse(input, out var unixTime)
                ? DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime
                : defaultValue;
        }
    }

}


