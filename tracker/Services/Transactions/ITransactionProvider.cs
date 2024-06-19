


using Tracker.Models;

namespace Tracker.Services.Transactions
{
    public interface ITransactionProvider
    {
        Task<List<Transaction>> GetTransactionsAsync(string walletAddress);
    }
}


