using KK.Common.Models;

namespace KK.Common.Repositories
{
    /// <summary>
    /// Defines methods for managing player wallets in the database.
    /// </summary>
    public interface IWalletRepository
    {
        /// <summary>
        /// Retrieves a player's wallet by their unique identifier.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <returns>The wallet associated with the player, or null if not found.</returns>
        Task<Wallet?> GetWalletAsync(Guid playerId);

        /// <summary>
        /// Creates a new wallet for a player.
        /// </summary>
        /// <param name="wallet">The wallet object to be created.</param>
        /// <returns>True if the wallet was created successfully; otherwise, false.</returns>
        Task<bool> CreateWalletAsync(Wallet wallet);

        /// <summary>
        /// Updates an existing player's wallet.
        /// </summary>
        /// <param name="wallet">The updated wallet object.</param>
        /// <returns>True if the wallet was updated successfully; otherwise, false.</returns>
        Task<bool> UpdateWalletAsync(Wallet wallet);

        /// <summary>
        /// Deletes a player's wallet by their unique identifier.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <returns>True if the wallet was deleted successfully; otherwise, false.</returns>
        Task<bool> DeleteWalletAsync(Guid playerId);

        /// <summary>
        /// Deposits a specified amount into a player's wallet.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <param name="amount">The amount to deposit.</param>
        /// <returns>True if the deposit was successful; otherwise, false.</returns>
        Task<bool> DepositAsync(Guid playerId, decimal amount);

        /// <summary>
        /// Withdraws a specified amount from a player's wallet.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <param name="amount">The amount to withdraw.</param>
        /// <returns>True if the withdrawal was successful; otherwise, false.</returns>
        Task<bool> WithdrawAsync(Guid playerId, decimal amount);
    }

}
