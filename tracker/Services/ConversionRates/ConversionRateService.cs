using Tracker.Models;

namespace Tracker.Services.ConversionRates
{
    public class ConversionRateService
    {
        private readonly IEnumerable<IConversionRateProvider> _priceProviders;
        private List<ConversionRate> _ratesCache = new List<ConversionRate>();

        public ConversionRateService(IEnumerable<IConversionRateProvider> priceProviders)
        {
            _priceProviders = priceProviders;
        }

        public async Task<ConversionRate?> GetConversionRateAsync(Token fromToken, Token toToken)
        {
            // Attempt to find rate in cache
            ConversionRate? rate = GetRateFromCache(fromToken, toToken);

            // If rate is found in cache and is stale, update it
            if (rate != null && IsRateStale(rate))
            {
                rate = await GetRateFromProviders(fromToken, toToken);
                if (rate != null)
                {
                    UpdateRateInCache(rate);
                }
            }
            // If rate is not found in cache, fetch from providers
            else if (rate == null)
            {
                rate = await GetRateFromProviders(fromToken, toToken);
                if (rate != null)
                {
                    CacheRate(rate);
                }
            }

            return rate;
        }

        private ConversionRate? GetRateFromCache(Token fromToken, Token toToken)
        {
            return _ratesCache.FirstOrDefault(r => r.FromToken == fromToken && r.ToToken == toToken);
        }

        private void UpdateRateInCache(ConversionRate rate)
        {
            var existingRate = _ratesCache.FirstOrDefault(r => r.FromToken == rate.FromToken && r.ToToken == rate.ToToken);
            if (existingRate != null)
            {
                existingRate.Rate = rate.Rate;
                existingRate.LastUpdated = rate.LastUpdated;
            }
            else
            {
                _ratesCache.Add(rate);
            }
        }

        private void CacheRate(ConversionRate rate)
        {
            _ratesCache.Add(rate);
        }

        private async Task<ConversionRate?> GetRateFromProviders(Token fromToken, Token toToken)
        {
            foreach (var provider in _priceProviders)
            {
                ConversionRate? rate = await provider.GetConversionRateAsync(fromToken, toToken);
                if (rate != null)
                {
                    return rate;
                }
            }
            return null;
        }

        private bool IsRateStale(ConversionRate rate)
        {
            return (DateTime.UtcNow - rate.LastUpdated) > TimeSpan.FromMinutes(1);
        }
    }
}


