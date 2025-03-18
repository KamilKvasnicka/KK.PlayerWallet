using KK.Common.Models;

namespace KK.WalletService.Services
{
    /// <summary>
    /// Defines methods for handling game-related wallet transactions.
    /// </summary>
    public interface IGameWalletService
    {
        /// <summary>
        /// Processes a stake/bet transaction.
        /// </summary>
        /// <param name="request">The transaction request containing player ID, amount, and transaction details.</param>
        /// <returns>A transaction response indicating success or failure.</returns>
        Task<TransactionResponse> StakeAsync(TransactionRequest request);

        /// <summary>
        /// Processes a win transaction for a player.
        /// </summary>
        /// <param name="request">The transaction request containing player ID, amount, and transaction details.</param>
        /// <returns>A transaction response indicating success or failure.</returns>
        Task<TransactionResponse> WinAsync(TransactionRequest request);

        /// <summary>
        /// Processes a request to end a non-winning game round.
        /// </summary>
        /// <param name="request">The transaction request containing player ID, amount, and transaction details.</param>
        /// <returns>A transaction response indicating success or failure.</returns>
        Task<TransactionResponse> EndGameRoundAsync(TransactionRequest request);
    }
}
