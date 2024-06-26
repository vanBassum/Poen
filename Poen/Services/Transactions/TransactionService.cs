﻿using Poen.Models;
using System.Text.Json;

namespace Poen.Services.Transactions
{
    public class TransactionService
    {
        private readonly IEnumerable<ITransactionProvider> _transactionProviders;

        public TransactionService(IEnumerable<ITransactionProvider> transactionProviders)
        {
            _transactionProviders = transactionProviders;
        }

        public async Task<List<Transaction>> GetTransactionsForAddressAsync(string address)
        {
            var transactions = new List<Transaction>();

            foreach (var provider in _transactionProviders)
            {
                transactions.AddRange(await provider.GetTransactionsAsync(address));
            }

            return transactions;
        }
    }
}


