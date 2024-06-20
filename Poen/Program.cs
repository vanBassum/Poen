using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Poen.Services.ConversionRates;
using Poen.Services;
using Poen.Services.Manual;
using Poen.Services.BlockChain;
using Poen.Services.Transactions;
using Poen.Services.CoinMarketCap;
using Poen.Config;
using System;


HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<ApplicationConfig>(builder.Configuration.GetSection("Application"));
builder.Services.Configure<BlockChainSettings>(builder.Configuration.GetSection("BlockChain"));
builder.Services.Configure<CoinMarketCapConfig>(builder.Configuration.GetSection("CoinMarketCap"));
builder.Services.Configure<InfluxDbConfig>(builder.Configuration.GetSection("InfluxDb"));


// Add HttpClient to the DI container
builder.Services.AddHttpClient();
builder.Services.AddSingleton<InfluxDBService>();
builder.Services.AddSingleton<TransactionService>();
builder.Services.AddSingleton<ConversionRateService>();
builder.Services.AddHostedService<ApplicationService>();

builder.Services.AddTransient<ITransactionProvider, BlockChainTransactionService>();
builder.Services.AddTransient<IConversionRateProvider, CoinMarketCapRateProvider>();
builder.Services.AddTransient<IConversionRateProvider, LookupConversionProvider>();


using IHost host = builder.Build();
await host.RunAsync();
