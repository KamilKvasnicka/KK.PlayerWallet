using KK.Common.Models;

namespace KK.Common.Repositories
{
    /// <summary>
    /// Defines methods for managing transactions in the database.
    /// </summary>
    public interface ITransactionRepository
    {
        /// <summary>
        /// Adds a new transaction to the database.
        /// </summary>
        /// <param name="transaction">The transaction to be added.</param>
        /// <returns>True if the transaction was successfully added, otherwise false.</returns>
        Task<bool> AddTransactionAsync(Transaction transaction);

        /// <summary>
        /// Retrieves all transactions for a specific player.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <returns>A list of transactions associated with the given player.</returns>
        Task<List<Transaction>> GetTransactionsByPlayerAsync(Guid playerId);

        /// <summary>
        /// Saves a new transaction with the specified player ID, remote transaction ID, and amount.
        /// </summary>
        /// <param name="playerId">The ID of the player.</param>
        /// <param name="txRemoteId">The unique transaction identifier for idempotency.</param>
        /// <param name="amount">The amount of the transaction.</param>
        /// <returns>True if the transaction was saved successfully, otherwise false.</returns>
        Task<bool> SaveTransaction(Guid playerId, string txRemoteId, decimal amount);

        /// <summary>
        /// Checks if a transaction with the given remote transaction ID already exists.
        /// </summary>
        /// <param name="txRemoteId">The remote transaction ID to check.</param>
        /// <returns>True if the transaction exists, otherwise false.</returns>
        Task<bool> TransactionExistsAsync(string txRemoteId);
    }

}
