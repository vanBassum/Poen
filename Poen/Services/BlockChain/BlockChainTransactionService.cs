using Microsoft.Extensions.Options;
using Poen.Config;
using Poen.Models;
using Poen.Services.Transactions;

namespace Poen.Services.BlockChain
{
    public class BlockChainTransactionService : ITransactionProvider
    {
        private readonly BlockChainSettings _config;
        private readonly List<BlockchainTransactionProvider> providers = new List<BlockchainTransactionProvider>();

        public BlockChainTransactionService(HttpClient httpClient, IOptions<BlockChainSettings> config)
        {
            _config = config.Value;

            foreach (var scannerConfig in _config.Scanners)
            {
                var providerInstance = new BlockchainTransactionProvider(httpClient, scannerConfig.Endpoint, scannerConfig.ApiKey);
                providers.Add(providerInstance);
            }
        }

        public async Task<List<Transaction>> GetTransactionsAsync(string walletAddress)
        {
            List<Transaction> transactions = new List<Transaction>();
            foreach (var provider in providers)
            {
                transactions.AddRange(await provider.GetTransactionsAsync(walletAddress));
            }
            return transactions;
        }
    }

}


