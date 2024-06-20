using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Tracker.Models;
using Tracker.Services.ConversionRates;
using Tracker.Services.Transactions;



namespace Tracker.Services
{
    public class ApplicationService : BackgroundService
    {
        private readonly TransactionService _transactionService;
        private readonly Wallets _wallets;
        private readonly Tokens _tokens;
        private readonly ConversionRateService _conversionRateService;
        private readonly InfluxDBService _influxDBService;

        public ApplicationService(TransactionService transactionService, IOptions<Wallets> wallets, IOptions<Tokens> tokens, ConversionRateService conversionRateService, InfluxDBService influxDBService)
        {
            _transactionService = transactionService;
            _wallets = wallets.Value;
            _tokens = tokens.Value;
            _conversionRateService = conversionRateService;
            _influxDBService = influxDBService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var wallet in _wallets.Bsc)
                    await HandleWalletBsc(wallet);
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
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

            Console.WriteLine("Token Balances:");
            Console.WriteLine(string.Format("{0,-30} {1,-20} {2,20} {3,20}", "Token Name", "Symbol", "Balance", "Usd"));
            Console.WriteLine(new string('-', 90)); // Adjusted separator line length for better formatting

            foreach (var balance in portofolio.OrderBy(a => a.Token.Symbol))
            {
                var rate = rates.FirstOrDefault(r => r.FromToken == balance.Token && r.ToToken == usdToken);
                decimal? usdValue = rate?.Price == null ? null : rate.Price * balance.Value;
                var usdValueString = usdValue.HasValue ? usdValue.Value.ToString("N4") : " ";

                Console.WriteLine(string.Format("{0,-30} {1,-20} {2,20:N4} {3,20:N4}",
                    ClipString(balance.Token.Name, nameMaxLength),
                    ClipString(balance.Token.Symbol, symbolMaxLength),
                    balance.Value,
                    usdValueString));

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
                        .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

                    details.Api.WritePoint(point, details.Bucket, details.Organisation);
                });
            }

            Console.WriteLine("");
            Console.WriteLine(string.Format("{0,-30} {1,-20} {2,20:N4} {3,20:N4}",
                "",
                "",
                "",
                totalUsd));
            Console.WriteLine("");
            return;

            Console.WriteLine("\nTransactions:");
            Console.WriteLine(string.Format("{0,-30} {1,-20} {2,20}", "Token Name", "Symbol", "Amount"));
            Console.WriteLine(new string('-', 70)); // Adjusted separator line length for better formatting

            foreach (var tx in transactions)
            {
                Console.WriteLine(string.Format("{0,-30} {1,-20} {2,20:N4}",
                    ClipString(tx.Token.Name, nameMaxLength),
                    ClipString(tx.Token.Symbol, symbolMaxLength),
                    tx.Value));
            }
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
                if (_tokens.Whitelist.Length > 0)
                {
                    if (!_tokens.Whitelist.Contains(transaction.Token.Symbol))
                        continue;
                }

                if (_tokens.Blacklist.Length > 0)
                {
                    if (_tokens.Blacklist.Any(a => transaction.Token.Symbol.StartsWith(a)))
                        continue;
                }

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




