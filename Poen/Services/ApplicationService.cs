﻿using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Poen.Config;
using Poen.Models;
using Poen.Services.CoinMarketCap;
using Poen.Services.ConversionRates;
using Poen.Services.Transactions;
using System;
using System.Text;

namespace Poen.Services
{
    public class ApplicationService : BackgroundService
    {
        private readonly TransactionService _transactionService;
        private readonly ApplicationConfig _applicationConfig;
        private readonly ConversionRateService _conversionRateService;
        private readonly InfluxDBService _influxDBService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApplicationService> _logger;

        public ApplicationService(TransactionService transactionService, ConversionRateService conversionRateService, InfluxDBService influxDBService, IConfiguration configuration, IOptions<ApplicationConfig> applicationConfig, ILogger<ApplicationService> logger)
        {
            _transactionService = transactionService;
            _conversionRateService = conversionRateService;
            _influxDBService = influxDBService;
            _configuration = configuration;
            _applicationConfig = applicationConfig.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            PrintConfig();
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_applicationConfig.Wallet == null)
                    throw new Exception("No wallet configured");
                await HandleWalletBsc(_applicationConfig.Wallet);
                await Task.Delay(TimeSpan.FromSeconds(_applicationConfig.ScanInterval), stoppingToken);
            }
        }


        void PrintConfig()
        {
            StringBuilder logMessage = new StringBuilder();
            logMessage.AppendLine("Configuration Settings:");
            logMessage.AppendLine("-----------------------");

            foreach (var kvp in _configuration.AsEnumerable())
            {
                logMessage.AppendLine($"{kvp.Key}: {kvp.Value}");
            }

            _logger.LogInformation(logMessage.ToString());
        }

        private async Task HandleWalletBsc(string wallet)
        {
            var transactions = await _transactionService.GetTransactionsForAddressAsync(wallet);
            var portofolio = CalculatePortofolio(wallet, transactions);

            const int nameMaxLength = 30;
            const int symbolMaxLength = 20;
            decimal totalUsd = 0;

            var usdToken = new Token { Contract = "", Name = "Dollar", Symbol = "USD" };
            var rates = await GetRatesForPortofolio(portofolio, usdToken);

            var logBuilder = new StringBuilder();
            var timestamp = DateTime.UtcNow;
            foreach (var balance in portofolio.OrderBy(a => a.Token.Symbol))
            {
                var rate = rates.FirstOrDefault(r => r.FromToken == balance.Token && r.ToToken == usdToken);
                decimal? usdValue = rate?.Price == null ? null : rate.Price * balance.Value;
                var usdValueString = usdValue.HasValue ? usdValue.Value.ToString("N4") : " ";

                totalUsd += usdValue ?? 0;

                _influxDBService.Write(details =>
                {
                    var point = PointData
                        .Measurement("Portofolio")
                        .Tag("Wallet", wallet)
                        .Tag("TokenSymbol", balance.Token.Symbol)
                        .Tag("TokenName", balance.Token.Name)
                        .Tag("TokenContract", balance.Token.Contract)
                        .Field("Value", balance.Value)
                        .Field("Usd", usdValue)
                        .Timestamp(timestamp, WritePrecision.Ns);

                    details.Api.WritePoint(point, details.Bucket, details.Organisation);
                });

                logBuilder.Append($"{balance.Token.Symbol} (${usdValue?.ToStringSI()}), ");
            }

            logBuilder.Append($"Total (${totalUsd.ToStringSI()})");

            _logger.LogInformation(logBuilder.ToString());

        }




        private string ClipString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input ?? string.Empty;
            }

            return input.Length > maxLength ? input.Substring(0, maxLength) : input;
        }

        private List<TokenBalance> CalculatePortofolio(string wallet, IEnumerable<Transaction> transactions)
        {
            List<TokenBalance> tokenBalances = new List<TokenBalance>();

            foreach (var transaction in transactions)
            {
                if (_applicationConfig.TokenBlacklist?.Any(a => transaction.Token.Symbol.StartsWith(a)) ?? false)
                        continue;

                TokenBalance? tokenBalance = tokenBalances.FirstOrDefault(b => b.Token == transaction.Token);
                if (tokenBalance == null)
                {
                    tokenBalance = new TokenBalance
                    {
                        Token = transaction.Token,
                        Value = 0
                    };
                    tokenBalances.Add(tokenBalance);
                }

                if (transaction.From.Equals(wallet, StringComparison.OrdinalIgnoreCase))
                {
                    // Outgoing transaction
                    tokenBalance.Value -= transaction.Value;
                }
                else if (transaction.To.Equals(wallet, StringComparison.OrdinalIgnoreCase))
                {
                    // Incoming transaction
                    tokenBalance.Value += transaction.Value;
                }
            }
            return tokenBalances;
        }

        async Task<List<ConversionRate>> GetRatesForPortofolio(List<TokenBalance> portofolio, Token toToken)
        {
            List<ConversionRate> rates = new List<ConversionRate>();

            foreach (var tokenBalance in portofolio)
            {
                rates.Add(new ConversionRate
                {
                    FromToken = tokenBalance.Token,
                    ToToken = toToken
                });
            }

            await _conversionRateService.UpdateConversionRates(rates);
            return rates;
        }
    }

    class TokenBalance
    {
        public required Token Token { get; set; }
        public decimal? Value { get; set; }
        public override string ToString() => $"{Token.Symbol}";
    }
}




