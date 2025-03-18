using KK.Common.Enums;
using KK.Common.Messaging;
using KK.Common.Models;
using KK.PlayerWalletApi.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("wallet")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<WalletController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WalletController"/> class.
    /// </summary>
    /// <param name="walletService">Service for managing wallet operations.</param>
    /// <param name="messageBus">Message bus for event-driven communication.</param>
    /// <param name="logger">Logger for tracking wallet transactions.</param>
    public WalletController(IWalletService walletService, IMessageBus messageBus, ILogger<WalletController> logger)
    {
        _walletService = walletService;
        _messageBus = messageBus;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new wallet for a player.
    /// </summary>
    /// <param name="request">The registration request containing the player ID.</param>
    /// <returns>Response indicating whether the wallet was successfully created.</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Register method called for Player {PlayerId}", request.PlayerId);

        var result = await _walletService.CreateWalletAsync(request.PlayerId);
        if (!result)
        {
            _logger.LogWarning("Wallet creation failed: Player {PlayerId} already has a wallet", request.PlayerId);
            return BadRequest(new { message = "Wallet already exists" });
        }

        _logger.LogInformation("Wallet successfully created for Player {PlayerId}", request.PlayerId);
        return Ok(new { message = "Wallet created" });
    }

    /// <summary>
    /// Processes a deposit transaction for a player.
    /// </summary>
    /// <param name="request">Transaction request.</param>
    /// <returns>Transaction response.</returns>
    [HttpPost("deposit")]
    public async Task<ActionResult<TransactionResponse>> Deposit([FromBody] TransactionRequest request)
    {
        request.Type = TransactionType.Deposit;
        var response = await _walletService.DepositAsync(request);

        if (!response.IsSucces)
        {
            _logger.LogWarning("Deposit failed for Player {PlayerId}, Status {StatusCode}", request.PlayerId, response.StatusCode);
            return BadRequest(response);
        }

        await _messageBus.PublishAsync("wallet.deposit", request);
        return Ok(response);
    }

    /// <summary>
    /// Processes a withdrawal transaction for a player.
    /// </summary>
    /// <param name="request">Transaction request.</param>
    /// <returns>Transaction response.</returns>
    [HttpPost("withdraw")]
    public async Task<ActionResult<TransactionResponse>> Withdraw([FromBody] TransactionRequest request)
    {
        request.Type = TransactionType.Withdraw;
        var response = await _walletService.WithdrawAsync(request);

        if (!response.IsSucces)
        {
            _logger.LogWarning("Withdraw failed: Insufficient funds for Player {PlayerId}, Status {StatusCode}", request.PlayerId, response.StatusCode);
            return BadRequest(response);
        }

        await _messageBus.PublishAsync("wallet.withdraw", request);
        return Ok(response);
    }

    /// <summary>
    /// Retrieves the balance of a player's wallet.
    /// </summary>
    /// <param name="playerId">GUID of the player.</param>
    /// <returns>The player's wallet balance or a not found response.</returns>
    [HttpGet("balance/{playerId}")]
    public async Task<IActionResult> GetBalance(Guid playerId)
    {
        _logger.LogInformation("Fetching balance for Player {PlayerId}", playerId);

        var balance = await _walletService.GetBalanceAsync(playerId);
        if (balance == null)
        {
            _logger.LogWarning("Balance request failed: Player {PlayerId} does not have a wallet", playerId);
            return NotFound(new { message = "Wallet not found" });
        }

        _logger.LogInformation("Balance retrieved for Player {PlayerId}: {Balance}", playerId, balance);
        return Ok(new { balance });
    }

    /// <summary>
    /// Retrieves the transaction history of a player.
    /// </summary>
    /// <param name="playerId">GUID of the player.</param>
    /// <returns>A list of transactions for the specified player.</returns>
    [HttpGet("transactions/{playerId}")]
    public async Task<IActionResult> GetTransactions(Guid playerId)
    {
        _logger.LogInformation("Fetching transactions for Player {PlayerId}", playerId);

        var transactions = await _walletService.GetTransactionsAsync(playerId);
        return Ok(transactions);
    }
}

