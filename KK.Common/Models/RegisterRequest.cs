namespace KK.Common.Models
{
    /// <summary>
    /// Represents a request to register a new wallet for a player.
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// The unique identifier of the player requesting wallet registration.
        /// </summary>
        public Guid PlayerId { get; set; }
    }
}
