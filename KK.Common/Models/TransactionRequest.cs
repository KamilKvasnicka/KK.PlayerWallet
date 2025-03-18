using KK.Common.Enums;

namespace KK.Common.Models
{
    /// <summary>
    /// Represents a request to process a financial transaction for a player's wallet.
    /// </summary>
    public class TransactionRequest
    {
        /// <summary>
        /// The unique identifier of the player making the transaction.
        /// </summary>
        public Guid PlayerId { get; set; }

        /// <summary>
        /// The unique remote transaction ID to ensure idempotency.
        /// </summary>
        public required string TxRemoteId { get; set; }

        /// <summary>
        /// The amount involved in the transaction (can be positive or negative).
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The type of the transaction (Deposit, Withdraw, Stake, etc.).
        /// </summary>
        public TransactionType Type { get; set; }
    }

}
