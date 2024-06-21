using Poen.Models;

namespace Poen.Services
{
    public class ConversionRateService
    {
        private readonly IEnumerable<IConversionRateProvider> _priceProviders;

        public ConversionRateService(IEnumerable<IConversionRateProvider> priceProviders)
        {
            _priceProviders = priceProviders;
        }

        public async Task UpdateConversionRates(List<ConversionRate> rates)
        {
            foreach (var provider in _priceProviders)
            {
                await provider.UpdateConversionRates(rates);
            }

        }
    }
}


