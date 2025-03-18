using KK.Common.Models;

namespace KK.PlayerWalletApi.Services
{
    /// <summary>
    /// Defines operations for managing player wallets and transactions.
    /// </summary>
    public interface IWalletService
    {
        /// <summary>
        /// Creates a new wallet for a player.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <returns>True if the wallet was created successfully; otherwise, false.</returns>
        Task<bool> CreateWalletAsync(Guid playerId);

        /// <summary>
        /// Deletes a player's wallet.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <returns>True if the wallet was deleted successfully; otherwise, false.</returns>
        Task<bool> DeleteWalletAsync(Guid playerId);

        /// <summary>
        /// Retrieves the balance of a player's wallet.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <returns>The current balance of the player's wallet, or null if the wallet does not exist.</returns>
        Task<decimal?> GetBalanceAsync(Guid playerId);

        /// <summary>
        /// Processes a deposit transaction for a player's wallet.
        /// </summary>
        /// <param name="request">The transaction request containing player ID, amount, and transaction details.</param>
        /// <returns>A transaction response indicating success or failure.</returns>
        Task<TransactionResponse> DepositAsync(TransactionRequest request);

        /// <summary>
        /// Processes a withdrawal transaction from a player's wallet.
        /// </summary>
        /// <param name="request">The transaction request containing player ID, amount, and transaction details.</param>
        /// <returns>A transaction response indicating success or failure.</returns>
        Task<TransactionResponse> WithdrawAsync(TransactionRequest request);

        /// <summary>
        /// Retrieves the transaction history for a specific player.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <returns>A list of transactions associated with the given player.</returns>
        Task<List<Transaction>> GetTransactionsAsync(Guid playerId);
    }

}
