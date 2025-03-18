using KK.Common.Enums;
using KK.Common.Models;
using KK.Common.Repositories;

namespace KK.WalletService.Services
{
    /// <summary>
    /// Handles game-related wallet transactions such as stake, win, and end game rounds.
    /// </summary>
    public class GameWalletService : IGameWalletService
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<GameWalletService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameWalletService"/> class.
        /// </summary>
        /// <param name="transactionService">Service for processing transactions.</param>
        /// <param name="logger">Logger for tracking game wallet operations.</param>
        public GameWalletService(ITransactionService transactionService, IWalletRepository @object, ILogger<GameWalletService> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        /// <summary>
        /// Processes a stake/bet transaction.
        /// Determines if the stake is a normal bet or a freespin.
        /// </summary>
        /// <param name="request">The transaction request containing player ID, amount, and transaction details.</param>
        /// <returns>A transaction response indicating success or failure.</returns>
        public async Task<TransactionResponse> StakeAsync(TransactionRequest request)
        {
            // Determine if the stake is a normal bet or a freespin
            request.Type = request.Type == TransactionType.Freespin ? TransactionType.Freespin : TransactionType.Stake;

            var response = await _transactionService.ProcessTransactionAsync(request);

            _logger.LogInformation("Stake processed: Player {PlayerId}, Amount {Amount}, TxRemoteId {TxRemoteId}, Status {StatusCode}",
                                   request.PlayerId, request.Amount, request.TxRemoteId, response.StatusCode);

            return response;
        }

        /// <summary>
        /// Processes a win transaction for a player.
        /// </summary>
        /// <param name="request">The transaction request containing player ID, amount, and transaction details.</param>
        /// <returns>A transaction response indicating success or failure.</returns>
        public async Task<TransactionResponse> WinAsync(TransactionRequest request)
        {
            request.Type = TransactionType.Win;
            var response = await _transactionService.ProcessTransactionAsync(request);

            _logger.LogInformation("Win processed: Player {PlayerId}, Amount {Amount}, TxRemoteId {TxRemoteId}, Status {StatusCode}",
                                   request.PlayerId, request.Amount, request.TxRemoteId, response.StatusCode);

            return response;
        }

        /// <summary>
        /// Processes a request to end a non-winning game round.
        /// </summary>
        /// <param name="request">The transaction request containing player ID, amount, and transaction details.</param>
        /// <returns>A transaction response indicating success or failure.</returns>
        public async Task<TransactionResponse> EndGameRoundAsync(TransactionRequest request)
        {
            request.Type = TransactionType.EndGameRound;
            var response = await _transactionService.ProcessTransactionAsync(request);

            _logger.LogInformation("EndGameRound processed: Player {PlayerId}, Amount {Amount}, TxRemoteId {TxRemoteId}, Status {StatusCode}",
                                   request.PlayerId, request.Amount, request.TxRemoteId, response.StatusCode);

            return response;
        }
    }
}
