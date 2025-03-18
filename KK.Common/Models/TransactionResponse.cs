using KK.Common.Enums;

namespace KK.Common.Models
{
    /// <summary>
    /// Represents the response returned after processing a financial transaction.
    /// </summary>
    public class TransactionResponse
    {
        /// <summary>
        /// Indicates whether the transaction was successfully processed.
        /// </summary>
        public bool IsSucces { get; set; }

        /// <summary>
        /// The status code representing the transaction result (e.g., success, insufficient funds, duplicate transaction).
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// The unique identifier of the player associated with the transaction.
        /// </summary>
        public Guid PlayerId { get; set; }

        /// <summary>
        /// The unique remote transaction ID used to ensure idempotency.
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
