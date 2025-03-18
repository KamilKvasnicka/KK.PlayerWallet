﻿using KK.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace KK.Common.Models
{
    /// <summary>
    /// Represents a financial transaction related to a player's wallet.
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// The unique identifier of the transaction.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// The unique remote transaction ID to ensure idempotency.
        /// </summary>
        public required string TxRemoteId { get; set; }

        /// <summary>
        /// The unique identifier of the player associated with the transaction.
        /// </summary>
        public Guid PlayerId { get; set; }

        /// <summary>
        /// The amount involved in the transaction (can be positive or negative).
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The type of the transaction (Deposit, Withdraw, Stake, etc.).
        /// </summary>
        public TransactionType Type { get; set; }

        /// <summary>
        /// The timestamp of when the transaction was created (UTC).
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
