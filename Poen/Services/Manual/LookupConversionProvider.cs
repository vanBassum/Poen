using Microsoft.Extensions.Options;
using Poen.Models;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace Poen.Services.Manual
{
    public class LookupConversionProvider : IConversionRateProvider
    {
        private readonly Dictionary<string, decimal> lookup = new Dictionary<string, decimal> {
            { "USD", 1 },
            { "USDT", 1 },
            { "BSC-USD", 1 },
        };


        public Task UpdateConversionRates(List<ConversionRate> rates)
        {
            foreach (ConversionRate rate in rates)
            {
                UpdateRate(rate);
            }
            return Task.CompletedTask;
        }

        void UpdateRate(ConversionRate rate)
        {
            if (lookup.TryGetValue(rate.FromToken.Symbol, out var from))
            {
                if (lookup.TryGetValue(rate.ToToken.Symbol, out var to))
                {
                    rate.Price = to / from;
                    rate.LastUpdated = DateTime.UtcNow;
                }
            }
        }
    }
}
