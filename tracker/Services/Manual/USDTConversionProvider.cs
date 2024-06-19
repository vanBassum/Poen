using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Tracker.Models;
using Tracker.Services.CoinMarketCap;
using Tracker.Services.ConversionRates;

namespace Tracker.Services.Manual
{
    public class USDTConversionProvider : IConversionRateProvider
    {
        public USDTConversionProvider()
        {

        }


        public async Task<ConversionRate?> GetConversionRateAsync(Token fromToken, Token toToken)
        {
            if (toToken.Symbol != "USD")
                return null;

            decimal? rate = null;

            if (fromToken.Symbol == "USDT")
                rate = 1;

            if (fromToken.Symbol == "BSC-USD")
                rate = 1;

            if (rate == null)
                return null;

            return await Task.FromResult(new ConversionRate
            {
                FromToken = fromToken,
                ToToken = toToken,
                LastUpdated = DateTime.Now,
                Rate = rate.Value,
            });
        }
    }
}
