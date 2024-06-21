using Poen.Models;

namespace Poen.Services
{
    public interface IConversionRateProvider
    {
        //Task<ConversionRate?> GetConversionRateAsync(Token fromToken, Token toToken);

        Task UpdateConversionRates(List<ConversionRate> rates);

    }


}


