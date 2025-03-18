using KK.Common.Enums;
using KK.Common.Models;
using KK.WalletService.Services;
using Microsoft.AspNetCore.Mvc;

namespace KK.WalletService.Controllers
{
    [ApiController]
    [Route("game")]
    public class GameController : ControllerBase
    {
        private readonly IGameWalletService _gameWalletService;
        private readonly ILogger<GameController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameController"/> class.
        /// </summary>
        /// <param name="gameWalletService">Service for managing game wallet transactions.</param>
        /// <param name="logger">Logger for logging transaction processing.</param>
        public GameController(IGameWalletService gameWalletService, ILogger<GameController> logger)
        {
            _gameWalletService = gameWalletService;
            _logger = logger;
        }

        /// <summary>
        /// Processes a stake/bet transaction for a player.
        /// </summary>
        /// <param name="request">Transaction request.</param>
        /// <param name="isFreespin">Indicates whether the stake is a freespin or a normal bet.</param>
        /// <returns>Transaction response.</returns>
        [HttpPost("stake")]
        public async Task<ActionResult<TransactionResponse>> Stake([FromBody] TransactionRequest request)
        {
            var response = await _gameWalletService.StakeAsync(request);

            if (!response.IsSucces)
            {
                _logger.LogWarning("Stake failed for Player {PlayerId}, Status {StatusCode}", request.PlayerId, response.StatusCode);
                return BadRequest(response);
            }

            _logger.LogInformation("Stake processed successfully: Player {PlayerId}, Amount {Amount}, TxRemoteId {TxRemoteId}, Status {StatusCode}",
                                   request.PlayerId, request.Amount, request.TxRemoteId, response.StatusCode);
            return Ok(response);
        }

        /// <summary>
        /// Processes a win transaction for a player.
        /// </summary>
        /// <param name="request">Transaction request.</param>
        /// <returns>Transaction response.</returns>
        [HttpPost("win")]
        public async Task<ActionResult<TransactionResponse>> Win([FromBody] TransactionRequest request)
        {
            request.Type = TransactionType.Win;
            var response = await _gameWalletService.WinAsync(request);

            if (!response.IsSucces)
            {
                _logger.LogWarning("Win failed for Player {PlayerId}, Status {StatusCode}", request.PlayerId, response.StatusCode);
                return BadRequest(response);
            }

            _logger.LogInformation("Win processed successfully: Player {PlayerId}, Amount {Amount}, TxRemoteId {TxRemoteId}, Status {StatusCode}",
                                   request.PlayerId, request.Amount, request.TxRemoteId, response.StatusCode);
            return Ok(response);
        }

        /// <summary>
        /// Ends a non-winning game round for a player.
        /// </summary>
        /// <param name="request">Transaction request.</param>
        /// <returns>Transaction response.</returns>
        [HttpPost("endgame")]
        public async Task<ActionResult<TransactionResponse>> EndGameRound([FromBody] TransactionRequest request)
        {
            request.Type = TransactionType.Win;
            var response = await _gameWalletService.EndGameRoundAsync(request);

            if (!response.IsSucces)
            {
                _logger.LogWarning("EndGame failed for Player {PlayerId}, Status {StatusCode}", request.PlayerId, response.StatusCode);
                return BadRequest(response);
            }

            _logger.LogInformation("EndGame processed successfully: Player {PlayerId}, Amount {Amount}, TxRemoteId {TxRemoteId}, Status {StatusCode}",
                                   request.PlayerId, request.Amount, request.TxRemoteId, response.StatusCode);
            return Ok(response);
        }
    }
}