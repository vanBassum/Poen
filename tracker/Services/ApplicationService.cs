using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Tracker.Services.Transactions;
using System.Linq;
using Tracker.Models;
using Tracker.Services.ConversionRates;



namespace Tracker.Services
{
    public class ApplicationService : BackgroundService
    {
        private readonly TransactionService _transactionService;
        private readonly Wallets _wallets;
        private readonly Tokens _tokens;
        private readonly ConversionRateService _conversionRateService;
        //private readonly DatabaseService _databaseService;

        public ApplicationService(TransactionService transactionService, IOptions<Wallets> wallets, IOptions<Tokens> tokens, ConversionRateService conversionRateService)
        {
            _transactionService = transactionService;
            _wallets = wallets.Value;
            _tokens = tokens.Value;
            _conversionRateService = conversionRateService;
            //_databaseService = databaseService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var wallet in _wallets.Bsc)
                    await HandleWalletBsc(wallet);
                await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken);
            }
        }

        private async Task HandleWalletBsc(string wallet)
        {
            var transactions = await _transactionService.GetTransactionsForAddressAsync(wallet);
            var balances = await CalculateBalance(wallet, transactions);

            const int nameMaxLength = 30;
            const int symbolMaxLength = 20;

            Console.WriteLine("Token Balances:");
            Console.WriteLine(string.Format("{0,-30} {1,-20} {2,20} {3,20}", "Token Name", "Symbol", "Balance", "Usd"));
            Console.WriteLine(new string('-', 90)); // Adjusted separator line length for better formatting

            foreach (var token in balances.OrderByDescending(a=>a.UsdValue))
            {
                var usdValue = token.UsdValue.HasValue ? token.UsdValue.Value.ToString("N4") : " ";

                Console.WriteLine(string.Format("{0,-30} {1,-20} {2,20:N4} {3,20:N4}",
                    ClipString(token.Token.Name, nameMaxLength),
                    ClipString(token.Token.Symbol, symbolMaxLength),
                    token.Value,
                    usdValue));
            }

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

        private async Task<List<DiplayBalance>> CalculateBalance(string wallet, IEnumerable<Transaction> transactions)
        {
            List<DiplayBalance> tokenBalances = new List<DiplayBalance>();

            foreach (var transaction in transactions)
            {
                if (_tokens.Whitelist.Length > 0)
                {
                    if (!_tokens.Whitelist.Contains(transaction.Token.Symbol))
                        continue;
                }

                if (_tokens.Blacklist.Length > 0)
                {
                    if (_tokens.Blacklist.Any(a=> transaction.Token.Symbol.StartsWith(a)))
                        continue;
                }

                DiplayBalance? tokenBalance = tokenBalances.FirstOrDefault(b => b.Token == transaction.Token);
                if (tokenBalance == null)
                {
                    tokenBalance = new DiplayBalance
                    {
                        Token = transaction.Token,
                        Value = 0,
                        UsdValue = null
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

            foreach (var balance in tokenBalances)
                await CalculateUsd(balance);

            return tokenBalances;
        }



        private async Task CalculateUsd(DiplayBalance balance)
        {
            Token usdToken = new Token { 
                Contract = "",
                Name = "Dollar",
                Symbol = "USD"
            };
            var rate = await _conversionRateService.GetConversionRateAsync(balance.Token, usdToken);

            if (rate == null)
                return;

            balance.UsdValue = rate.Rate * balance.Value;
        }
    }

    class DiplayBalance
    { 
        public required Token Token { get; set; }
        public decimal? Value { get; set; }
        public decimal? UsdValue { get; set; }

        public override string ToString() => $"{Token.Symbol} {UsdValue?.ToString("D2") ?? "n/a"}";
    }


}


