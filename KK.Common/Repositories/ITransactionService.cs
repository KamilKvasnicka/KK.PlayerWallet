using KK.Common.Enums;
using KK.Common.Models;

namespace KK.Common.Repositories
{
    public interface ITransactionService
    {
        /// <summary>
        /// Processes a transaction (stake, win, deposit)
        /// </summary>
        Task<TransactionResponse> ProcessTransactionAsync(TransactionRequest request);

        /// <summary>
        /// Retrieves a list of transactions for a given player
        /// </summary>
        Task<List<Transaction>> GetTransactionsByPlayerAsync(Guid playerId);

        /// <summary>
        /// Adds a new transaction to the database
        /// </summary>
        Task<bool> AddTransactionAsync(Transaction transaction);
    }

}
