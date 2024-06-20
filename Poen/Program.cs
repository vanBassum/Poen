using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Poen.Services.ConversionRates;
using Poen.Services;
using Poen.Services.Manual;
using Poen.Services.BlockChain;
using Poen.Services.Transactions;
using Poen.Services.CoinMarketCap;


HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<ApiKeys>(builder.Configuration.GetSection("ApiKeys"));
builder.Services.Configure<Wallets>(builder.Configuration.GetSection("Wallets"));
builder.Services.Configure<Tokens>(builder.Configuration.GetSection("Tokens"));
builder.Services.Configure<InfluxDb>(builder.Configuration.GetSection("InfluxDb"));

// Add HttpClient to the DI container
builder.Services.AddHttpClient();

builder.Services.AddSingleton<InfluxDBService>();
builder.Services.AddSingleton<TransactionService>();
builder.Services.AddSingleton<ConversionRateService>();
builder.Services.AddHostedService<ApplicationService>();

builder.Services.AddBlockChainTransactionProvider((provider, config) => {
    var apiKeys = provider.GetRequiredService<IOptions<ApiKeys>>();
    config.ApiKey = apiKeys.Value.Etherscan;
    config.BaseUrl = "https://api.etherscan.io/api";
});

builder.Services.AddBlockChainTransactionProvider((provider, config) => {
    var apiKeys = provider.GetRequiredService<IOptions<ApiKeys>>();
    config.ApiKey = apiKeys.Value.Bscscan;
    config.BaseUrl = "https://api.bscscan.com/api";
});

builder.Services.AddTransient<IConversionRateProvider, CoinMarketCapRateProvider>();
builder.Services.AddTransient<IConversionRateProvider, LookupConversionProvider>();

using IHost host = builder.Build();

await host.RunAsync();
