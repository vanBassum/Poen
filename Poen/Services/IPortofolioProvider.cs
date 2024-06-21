using Poen.Models;

namespace Poen.Services
{
    public interface IPortofolioProvider
    {
        Task UpdateBalances(Portofolio portofolio);
    }
}


