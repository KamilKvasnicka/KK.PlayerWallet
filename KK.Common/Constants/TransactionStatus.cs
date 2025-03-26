/// <summary>
/// Defines status codes for transaction processing results.
/// </summary>
public static class TransactionStatus
{
    /// <summary>
    /// A general error occurred during the transaction.
    /// </summary>
    public const int GeneralError = 0;

    /// <summary>
    /// The transaction was successfully processed.
    /// </summary>
    public const int Success = 1;

    /// <summary>
    /// The transaction was already processed and is considered a duplicate.
    /// </summary>
    public const int DuplicateTransaction = 2;

    /// <summary>
    /// The transaction failed due to insufficient funds.
    /// </summary>
    public const int InsufficientFunds = 3;

    /// <summary>
    /// The transaction failed because the player's wallet was not found.
    /// </summary>
    public const int WalletNotFound = 4;

    /// <summary>
    /// Concurrency conflict in database.
    /// </summary>
    public const int ConcurrencyConflict = 5;
}

