using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Poen.Config;
using Poen.Services.Transactions;
using System.Net.Http;

namespace Poen.Services.BlockChain
{
    public static class BlockChainScannerFactory
    {

        public static void CreateBlockChainScanners(this IServiceCollection services)
        {
            var blockchainSettings = services.BuildServiceProvider().GetRequiredService<IOptions<BlockChainSettings>>().Value;
            var httpClient = services.BuildServiceProvider().GetRequiredService<HttpClient>();

            if (blockchainSettings.Scanners == null || blockchainSettings.Scanners.Count == 0)
            {
                throw new ArgumentException("BlockChainSettings.Scanners must be provided and cannot be null or empty.");
            }

            foreach (var scanner in blockchainSettings.Scanners)
            {
                if (string.IsNullOrEmpty(scanner.ApiKey))
                {
                    throw new ArgumentNullException(nameof(scanner.ApiKey), "ApiKey in BlockChainSettings.Scanners must be provided and cannot be null or empty.");
                }

                if (string.IsNullOrEmpty(scanner.Endpoint))
                {
                    throw new ArgumentNullException(nameof(scanner.Endpoint), "Endpoint in BlockChainSettings.Scanners must be provided and cannot be null or empty.");
                }

                services.AddTransient<BlockchainTransactionProvider>(provider =>
                {
                    var httpClient = provider.GetRequiredService<HttpClient>();

                    // Instantiate and configure BlockchainTransactionProvider
                    return new BlockchainTransactionProvider(httpClient, scanner.Endpoint, scanner.ApiKey);
                });
            }
        }
    }
}
