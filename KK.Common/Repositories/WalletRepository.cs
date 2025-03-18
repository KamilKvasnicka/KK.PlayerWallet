using KK.Common.Models;
using KK.Common.DataStorage;

namespace KK.Common.Repositories
{
    /// <summary>
    /// Repository for managing player wallets in the database.
    /// </summary>
    public class WalletRepository : IWalletRepository
    {
        private readonly WalletDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletRepository"/> class.
        /// </summary>
        /// <param name="context">The database context for accessing wallet data.</param>
        public WalletRepository(WalletDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a player's wallet from the database.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <returns>The wallet associated with the player, or null if not found.</returns>
        public async Task<Wallet?> GetWalletAsync(Guid playerId)
        {
            return await _context.Wallets.FindAsync(playerId);
        }

        /// <summary>
        /// Creates a new wallet for a player.
        /// </summary>
        /// <param name="wallet">The wallet object to be created.</param>
        /// <returns>True if the wallet was created successfully; otherwise, false.</returns>
        public async Task<bool> CreateWalletAsync(Wallet wallet)
        {
            await _context.Wallets.AddAsync(wallet);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Updates an existing wallet in the database.
        /// </summary>
        /// <param name="wallet">The updated wallet object.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateWalletAsync(Wallet wallet)
        {
            _context.Wallets.Update(wallet);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Deletes a player's wallet from the database.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <returns>True if the wallet was deleted successfully; otherwise, false.</returns>
        public async Task<bool> DeleteWalletAsync(Guid playerId)
        {
            var wallet = await _context.Wallets.FindAsync(playerId);
            if (wallet == null) return false;

            _context.Wallets.Remove(wallet);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Deposits a specified amount into a player's wallet.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <param name="amount">The amount to deposit.</param>
        /// <returns>True if the deposit was successful; otherwise, false.</returns>
        public async Task<bool> DepositAsync(Guid playerId, decimal amount)
        {
            var wallet = await _context.Wallets.FindAsync(playerId);
            if (wallet == null) return false;

            wallet.Balance += amount;
            return await UpdateWalletAsync(wallet);
        }

        /// <summary>
        /// Withdraws a specified amount from a player's wallet, ensuring sufficient balance.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <param name="amount">The amount to withdraw.</param>
        /// <returns>True if the withdrawal was successful; otherwise, false.</returns>
        public async Task<bool> WithdrawAsync(Guid playerId, decimal amount)
        {
            var wallet = await _context.Wallets.FindAsync(playerId);
            if (wallet == null || wallet.Balance < amount) return false;

            wallet.Balance -= amount;
            return await UpdateWalletAsync(wallet);
        }
    }
}
