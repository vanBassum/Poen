using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using Tracker.Services.BlockChain;
using Tracker.Services.Transactions;

namespace Tracker.Services.BinanceSmartChain
{


    public static class BlockChainTransactionProviderExtentions
    {
        public static IServiceCollection AddBlockChainTransactionProvider(this IServiceCollection services, Action<IServiceProvider, BlockChainTransactionProviderConfig> configurator)
        {
            services.AddTransient<ITransactionProvider>(provider =>
            {
                var httpClient = provider.GetRequiredService<HttpClient>();
                var config = new BlockChainTransactionProviderConfig();
                configurator.Invoke(provider, config);

                return new BlockchainTransactionProvider(httpClient, config);
            });
            return services;
        }
    }
}