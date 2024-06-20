using Poen.Models;

namespace Poen.Services.ConversionRates
{
    public interface IConversionRateProvider
    {
        //Task<ConversionRate?> GetConversionRateAsync(Token fromToken, Token toToken);

        Task UpdateConversionRates(List<ConversionRate> rates);

    }


}


