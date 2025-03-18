using KK.Common.Enums;
using KK.Common.Models;
using KK.Common.Repositories;

namespace KK.PlayerWalletApi.Services
{
    /// <summary>
    /// Service for managing player wallets and processing transactions.
    /// </summary>
    public class WalletService : IWalletService
    {
        private readonly ITransactionService _transactionService;
        private readonly IWalletRepository _walletRepository;
        private readonly ILogger<WalletService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletService"/> class.
        /// </summary>
        /// <param name="transactionService">Service for processing transactions.</param>
        /// <param name="walletRepository">Repository for accessing wallet data.</param>
        /// <param name="logger">Logger for tracking wallet operations.</param>
        public WalletService(ITransactionService transactionService,
                             IWalletRepository walletRepository,
                             ILogger<WalletService> logger)
        {
            _transactionService = transactionService;
            _walletRepository = walletRepository;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new wallet for a player if one does not already exist.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <returns>True if the wallet was created successfully; otherwise, false.</returns>
        public async Task<bool> CreateWalletAsync(Guid playerId)
        {
            var existingWallet = await _walletRepository.GetWalletAsync(playerId);
            if (existingWallet != null)
            {
                _logger.LogWarning("Wallet creation failed: Player {PlayerId} already has a wallet.", playerId);
                return false;
            }

            var wallet = new Wallet { PlayerId = playerId, Balance = 0, CreatedAt = DateTime.UtcNow };
            var result = await _walletRepository.CreateWalletAsync(wallet);

            if (result)
            {
                _logger.LogInformation("Wallet created successfully for Player {PlayerId}.", playerId);
            }

            return result;
        }

        /// <summary>
        /// Deletes a player's wallet if it exists.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <returns>True if the wallet was deleted successfully; otherwise, false.</returns>
        public async Task<bool> DeleteWalletAsync(Guid playerId)
        {
            var result = await _walletRepository.DeleteWalletAsync(playerId);

            if (result)
            {
                _logger.LogInformation("Wallet deleted successfully for Player {PlayerId}.", playerId);
            }
            else
            {
                _logger.LogWarning("Failed to delete wallet: Player {PlayerId} not found.", playerId);
            }

            return result;
        }

        /// <summary>
        /// Retrieves the balance of a player's wallet.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <returns>The current balance of the player's wallet, or null if the wallet does not exist.</returns>
        public async Task<decimal?> GetBalanceAsync(Guid playerId)
        {
            var wallet = await _walletRepository.GetWalletAsync(playerId);
            if (wallet == null)
            {
                _logger.LogWarning("Balance request failed: Player {PlayerId} does not have a wallet.", playerId);
                return null;
            }

            _logger.LogInformation("Balance retrieved for Player {PlayerId}: {Balance}.", playerId, wallet.Balance);
            return wallet.Balance;
        }

        /// <summary>
        /// Processes a deposit transaction for a player's wallet.
        /// </summary>
        /// <param name="request">The transaction request containing player ID, amount, and transaction details.</param>
        /// <returns>A transaction response indicating success or failure.</returns>
        public async Task<TransactionResponse> DepositAsync(TransactionRequest request)
        {
            request.Type = TransactionType.Deposit;
            var response = await _transactionService.ProcessTransactionAsync(request);

            _logger.LogInformation("Deposit processed: Player {PlayerId}, Amount {Amount}, TxRemoteId {TxRemoteId}, Status {StatusCode}",
                                   request.PlayerId, request.Amount, request.TxRemoteId, response.StatusCode);

            return response;
        }

        /// <summary>
        /// Processes a withdrawal transaction from a player's wallet.
        /// </summary>
        /// <param name="request">The transaction request containing player ID, amount, and transaction details.</param>
        /// <returns>A transaction response indicating success or failure.</returns>
        public async Task<TransactionResponse> WithdrawAsync(TransactionRequest request)
        {
            request.Type = TransactionType.Withdraw;
            var response = await _transactionService.ProcessTransactionAsync(request);

            _logger.LogInformation("Withdraw processed: Player {PlayerId}, Amount {Amount}, TxRemoteId {TxRemoteId}, Status {StatusCode}",
                                   request.PlayerId, request.Amount, request.TxRemoteId, response.StatusCode);

            return response;
        }

        /// <summary>
        /// Retrieves the transaction history for a specific player.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <returns>A list of transactions associated with the given player.</returns>
        public async Task<List<Transaction>> GetTransactionsAsync(Guid playerId)
        {
            _logger.LogInformation("Fetching transactions for Player {PlayerId}", playerId);
            return await _transactionService.GetTransactionsByPlayerAsync(playerId);
        }
    }

}
