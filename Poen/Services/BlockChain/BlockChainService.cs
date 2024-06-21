using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Poen.Config;
using Poen.Models;

namespace Poen.Services.BlockChain
{
    public class BlockChainService : IPortofolioProvider
    {
        private readonly ILogger<BlockChainService> _logger;
        private readonly BlockChainSettings _config;
        private readonly List<BlockChainApi> clients = new List<BlockChainApi>();

        public BlockChainService(HttpClient httpClient, ILogger<BlockChainApi> clientLogger, ILogger<BlockChainService> logger, IOptions<BlockChainSettings> config)
        {
            _logger = logger;
            _config = config.Value;


            foreach (var scannerConfig in _config.Scanners)
            {
                if (scannerConfig.Endpoint == null || scannerConfig.ApiKey == null)
                    throw new Exception();

                clients.Add(new BlockChainApi(httpClient, scannerConfig.Endpoint, scannerConfig.ApiKey, clientLogger));
            }
        }

        public async Task UpdateBalances(Portofolio portofolio)
        {
            foreach(var balance in portofolio.Balances)
            {
                await UpdateBalance(balance, portofolio.Address);
            }
        }

        async Task UpdateBalance(TokenBalance balance, string walletAddress)
        {
            foreach (var client in clients)
            {
                var response = await client.GetBalance(balance.Token, walletAddress);
                decimal? value = ToDecimal(response?.result);
                if (value.HasValue)
                {
                    balance.Balance = value.Value;
                }
            }
        }

        public static decimal? ToDecimal(string? input, decimal? defaultValue = null)
        {
            return decimal.TryParse(input, out var result) ? result : defaultValue;
        }
    }
}


