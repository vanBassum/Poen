using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Poen.Services.Transactions;
using System.Net;

namespace Poen.Services.BlockChain
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