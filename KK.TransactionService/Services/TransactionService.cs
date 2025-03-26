using KK.Common.DataStorage;
using KK.Common.Enums;
using KK.Common.Messaging;
using KK.Common.Messaging.Events;
using KK.Common.Models;
using KK.Common.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KK.TransactionService.Services
{
    /// <summary>
    /// Service for handling player transactions, ensuring idempotency and balance consistency.
    /// </summary>
    public class TransactionService : ITransactionService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMessageBus _messageBus;
        private readonly WalletDbContext _dbContext;
        private readonly ILogger<TransactionService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionService"/> class.
        /// </summary>
        /// <param name="walletRepository">Repository for accessing wallet data.</param>
        /// <param name="transactionRepository">Repository for managing transactions.</param>
        /// <param name="messageBus">Message bus for publishing transaction events.</param>
        /// <param name="dbContext">Database context for transaction management.</param>
        /// <param name="logger">Logger for tracking transaction operations.</param>
        public TransactionService(IWalletRepository walletRepository,
                                  ITransactionRepository transactionRepository,
                                  IMessageBus messageBus,
                                  WalletDbContext dbContext,
                                  ILogger<TransactionService> logger)
        {
            _walletRepository = walletRepository;
            _transactionRepository = transactionRepository;
            _messageBus = messageBus;
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Processes a financial transaction, ensuring idempotency and balance validation.
        /// </summary>
        /// <param name="request">The transaction request containing player ID, amount, and transaction details.</param>
        /// <returns>A transaction response indicating success or failure with an appropriate status code.</returns>
        public async Task<TransactionResponse> ProcessTransactionAsync(TransactionRequest request)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    // Prevent amount with more as 2 decimal places e.g for EUR currency
                    if (decimal.Round(request.Amount, 2) != request.Amount)
                    {
                        _logger.LogWarning("Transaction rejected: Amount {Amount} has more than 2 decimal places.", request.Amount);
                        return new TransactionResponse
                        {
                            IsSucces = false,
                            StatusCode = TransactionStatus.GeneralError,
                            PlayerId = request.PlayerId,
                            TxRemoteId = request.TxRemoteId,
                            Amount = request.Amount,
                            Type = request.Type
                        };
                    }

                    // Prevent duplicate transactions
                    if (await _transactionRepository.TransactionExistsAsync(request.TxRemoteId))
                    {
                        _logger.LogWarning("Duplicate transaction detected: TxRemoteId {TxRemoteId} for Player {PlayerId}.", request.TxRemoteId, request.PlayerId);
                        return new TransactionResponse
                        {
                            IsSucces = true,
                            StatusCode = TransactionStatus.DuplicateTransaction,
                            PlayerId = request.PlayerId,
                            TxRemoteId = request.TxRemoteId,
                            Amount = request.Amount,
                            Type = request.Type
                        };
                    }
                    var wallet = await _dbContext.Wallets
                            .Where(w => w.PlayerId == request.PlayerId)
                            .FirstOrDefaultAsync();

                    if (wallet == null)
                    {
                        _logger.LogWarning("Transaction failed: Wallet not found for Player {PlayerId}.", request.PlayerId);
                        return new TransactionResponse
                        {
                            IsSucces = false,
                            StatusCode = TransactionStatus.WalletNotFound,
                            PlayerId = request.PlayerId,
                            TxRemoteId = request.TxRemoteId,
                            Amount = request.Amount,
                            Type = request.Type
                        };
                    }
                  
                    // Validate sufficient balance for withdrawal or stake transactions
                    if (request.Type != TransactionType.Freespin)
                    {
                        if (wallet.Balance < request.Amount && (request.Type == TransactionType.Withdraw || request.Type == TransactionType.Stake))
                        {
                            _logger.LogWarning("Insufficient funds: Player {PlayerId} attempted {Type} {Amount}, but only has {Balance}.",
                                               request.PlayerId, request.Type, request.Amount, wallet.Balance);
                            return new TransactionResponse
                            {
                                IsSucces = false,
                                StatusCode = TransactionStatus.InsufficientFunds,
                                PlayerId = request.PlayerId,
                                TxRemoteId = request.TxRemoteId,
                                Amount = request.Amount,
                                Type = request.Type
                            };
                        }

                        // Update balance based on transaction type
                        wallet.Balance += (request.Type == TransactionType.Stake || request.Type == TransactionType.Withdraw) ? -request.Amount : request.Amount;
                        await _walletRepository.UpdateWalletAsync(wallet);
                    }

                    // Create & store transaction record
                    var transactionRecord = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        TxRemoteId = request.TxRemoteId,
                        PlayerId = request.PlayerId,
                        Amount = request.Amount,
                        Type = request.Type,
                        Timestamp = DateTime.UtcNow
                    };

                    await _transactionRepository.AddTransactionAsync(transactionRecord);

                    try
                    {
                        await _dbContext.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        _logger.LogWarning("Concurrency conflict while updating Wallet for Player {PlayerId}", request.PlayerId);
                        await transaction.RollbackAsync();
                        return new TransactionResponse
                        {
                            IsSucces = false,
                            StatusCode = TransactionStatus.ConcurrencyConflict,
                            PlayerId = request.PlayerId,
                            TxRemoteId = request.TxRemoteId,
                            Amount = request.Amount,
                            Type = request.Type
                        };
                    }

                    _logger.LogInformation("Transaction successful: Player {PlayerId} {Type} {Amount}. New balance: {Balance}",
                                           request.PlayerId, request.Type, request.Amount, wallet.Balance);

                    await transaction.CommitAsync();

                    // Publish transaction event
                    var walletUpdateEvent = new WalletUpdateEvent(request.PlayerId, request.TxRemoteId, request.Amount);
                    await _messageBus.PublishAsync("wallet_updates", walletUpdateEvent);
                    _logger.LogInformation("Event published: Wallet update for Player {PlayerId}, Amount: {Amount}", request.PlayerId, request.Amount);
                    
                    return new TransactionResponse
                    {
                        IsSucces = true,
                        StatusCode = TransactionStatus.Success,
                        PlayerId = request.PlayerId,
                        TxRemoteId = request.TxRemoteId,
                        Amount = request.Amount,
                        Type = request.Type
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Transaction failed for Player {PlayerId}", request.PlayerId);
                    await transaction.RollbackAsync();
                    return new TransactionResponse
                    {
                        IsSucces = false,
                        StatusCode = TransactionStatus.GeneralError,
                        PlayerId = request.PlayerId,
                        TxRemoteId = request.TxRemoteId,
                        Amount = request.Amount,
                        Type = request.Type
                    };
                }
            });
        }

        /// <summary>
        /// Retrieves the transaction history for a specific player.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <returns>A list of transactions associated with the given player.</returns>
        public async Task<List<Transaction>> GetTransactionsByPlayerAsync(Guid playerId)
        {
            _logger.LogInformation("Fetching transactions for Player {PlayerId}", playerId);
            return await _transactionRepository.GetTransactionsByPlayerAsync(playerId);
        }

        /// <summary>
        /// Adds a new transaction to the transaction history.
        /// </summary>
        /// <param name="transaction">The transaction to be added.</param>
        /// <returns>True if the transaction was successfully added; otherwise, false.</returns>
        public async Task<bool> AddTransactionAsync(Transaction transaction)
        {
            _logger.LogInformation("Storing new transaction: Player {PlayerId}, Amount {Amount}, Type {Type}",
                                   transaction.PlayerId, transaction.Amount, transaction.Type);
            return await _transactionRepository.AddTransactionAsync(transaction);
        }
    }

}
