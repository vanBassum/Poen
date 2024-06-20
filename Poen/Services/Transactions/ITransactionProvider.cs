


using Poen.Models;

namespace Poen.Services.Transactions
{
    public interface ITransactionProvider
    {
        Task<List<Transaction>> GetTransactionsAsync(string walletAddress);
    }
}


