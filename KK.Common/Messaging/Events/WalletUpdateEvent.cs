namespace KK.Common.Messaging.Events
{
    public class WalletUpdateEvent
    {
        public Guid PlayerId { get; set; }
        public string TxRemoteId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public WalletUpdateEvent(Guid playerId, string txRemoteId, decimal amount)
        {
            PlayerId = playerId;
            TxRemoteId = txRemoteId;
            Amount = amount;
            Timestamp = DateTime.UtcNow;
        }
    }
}
