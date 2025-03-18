using KK.Common.DataStorage;
using KK.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KK.Common.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly WalletDbContext _dbContext;
        private readonly ILogger<TransactionRepository> _logger;

        public TransactionRepository(WalletDbContext dbContext, ILogger<TransactionRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new transaction if it does not already exist.
        /// </summary>
        public async Task<bool> AddTransactionAsync(Transaction transaction)
        {
            if (await TransactionExistsAsync(transaction.TxRemoteId))
            {
                _logger.LogWarning("Duplicate transaction detected: TxRemoteId {TxRemoteId}, Player {PlayerId}",
                                   transaction.TxRemoteId, transaction.PlayerId);
                return false;
            }

            await _dbContext.Transactions.AddAsync(transaction);
            return await SaveChangesWithLogging(transaction);
        }

        /// <summary>
        /// Retrieves transactions for a specific player.
        /// </summary>
        public async Task<List<Transaction>> GetTransactionsByPlayerAsync(Guid playerId)
        {
            return await _dbContext.Transactions
                                   .Where(t => t.PlayerId == playerId)
                                   .OrderByDescending(t => t.Timestamp)
                                   .ToListAsync();
        }

        /// <summary>
        /// Saves a new transaction with unique TxRemoteId.
        /// </summary>
        public async Task<bool> SaveTransaction(Guid playerId, string txRemoteId, decimal amount)
        {
            if (await TransactionExistsAsync(txRemoteId))
            {
                _logger.LogWarning("Transaction already exists: TxRemoteId {TxRemoteId}, Player {PlayerId}", txRemoteId, playerId);
                return false;
            }

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                TxRemoteId = txRemoteId,
                PlayerId = playerId,
                Amount = amount,
                Timestamp = DateTime.UtcNow
            };

            await _dbContext.Transactions.AddAsync(transaction);
            return await SaveChangesWithLogging(transaction);
        }

        /// <summary>
        /// Checks if a transaction with the given TxRemoteId already exists.
        /// </summary>
        public async Task<bool> TransactionExistsAsync(string txRemoteId)
        {
            return await _dbContext.Transactions.AnyAsync(t => t.TxRemoteId == txRemoteId);
        }

        /// <summary>
        /// Saves changes to the database and logs success/failure.
        /// </summary>
        private async Task<bool> SaveChangesWithLogging(Transaction transaction)
        {
            var result = await _dbContext.SaveChangesAsync() > 0;

            if (result)
            {
                _logger.LogInformation("Transaction saved: TxRemoteId {TxRemoteId}, Player {PlayerId}, Amount {Amount}",
                                       transaction.TxRemoteId, transaction.PlayerId, transaction.Amount);
            }
            else
            {
                _logger.LogError("Failed to save transaction: TxRemoteId {TxRemoteId}, Player {PlayerId}",
                                 transaction.TxRemoteId, transaction.PlayerId);
            }

            return result;
        }
    }
}

