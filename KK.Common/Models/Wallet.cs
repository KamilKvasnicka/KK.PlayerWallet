using System.ComponentModel.DataAnnotations;

namespace KK.Common.Models
{
    /// <summary>
    /// Represents a player's wallet, storing balance and creation details.
    /// </summary>
    public class Wallet
    {
        /// <summary>
        /// The unique identifier of the player associated with this wallet.
        /// </summary>
        [Key]
        public Guid PlayerId { get; set; }

        /// <summary>
        /// The current balance available in the wallet.
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// The date and time when the wallet was created (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
