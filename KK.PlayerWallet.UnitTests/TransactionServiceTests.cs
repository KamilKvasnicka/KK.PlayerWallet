namespace KK.PlayerWallet.UnitTests
{
    using Xunit;
    using Moq;
    using KK.Common.Repositories;
    using KK.Common.Models;
    using KK.TransactionService.Services;
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using KK.Common.Enums;
    using KK.Common.DataStorage;
    using Microsoft.Extensions.Logging.Abstractions;
    using KK.PlayerWalletApi.Services;
    using MyMessageBus = KK.Common.Messaging.IMessageBus;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;

    public class TransactionServiceTests
    {
        private readonly Mock<IWalletRepository> _walletRepositoryMock = new Mock<IWalletRepository>();
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock = new Mock<ITransactionRepository>();
        private readonly Mock<MyMessageBus> _messageBusMock = new Mock<MyMessageBus>();
        private readonly WalletDbContext _dbContext;
        private readonly TransactionService _transactionService;

        public TransactionServiceTests()
        {
            var options = new DbContextOptionsBuilder<WalletDbContext>()
     .UseInMemoryDatabase(Guid.NewGuid().ToString())
     .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
     .Options;
            _dbContext = new WalletDbContext(options);
            _transactionService = new TransactionService(
                _walletRepositoryMock.Object,
                _transactionRepositoryMock.Object,
                _messageBusMock.Object,
                _dbContext,
                new NullLogger<TransactionService>()
            );
        }

        [Fact]
        public async Task ProcessTransactionAsync_ShouldDecreaseBalance_WhenStake()
        {
            var playerId = Guid.NewGuid();
            var txRemoteId = "tx123";
            var amount = 50m;
            var wallet = new Wallet { PlayerId = playerId, Balance = 100m };
            _dbContext.Wallets.Add(wallet);
            await _dbContext.SaveChangesAsync();

            _walletRepositoryMock.Setup(r => r.GetWalletAsync(playerId)).ReturnsAsync(wallet);
            _transactionRepositoryMock.Setup(r => r.TransactionExistsAsync(txRemoteId)).ReturnsAsync(false);

            var result = await _transactionService.ProcessTransactionAsync(new TransactionRequest
            {
                PlayerId = playerId,
                TxRemoteId = txRemoteId,
                Amount = amount,
                Type = TransactionType.Stake
            });

            result.IsSucces.Should().BeTrue();
            wallet.Balance.Should().Be(50);
        }

        [Fact]
        public async Task ProcessTransactionAsync_ShouldReject_WhenInsufficientFunds()
        {
            var playerId = Guid.NewGuid();
            var txRemoteId = "tx124";
            var amount = 150m;
            var wallet = new Wallet { PlayerId = playerId, Balance = 100m };
            _dbContext.Wallets.Add(wallet);
            await _dbContext.SaveChangesAsync();

            _walletRepositoryMock.Setup(r => r.GetWalletAsync(playerId)).ReturnsAsync(wallet);
            _transactionRepositoryMock.Setup(r => r.TransactionExistsAsync(txRemoteId)).ReturnsAsync(false);

            var result = await _transactionService.ProcessTransactionAsync(new TransactionRequest
            {
                PlayerId = playerId,
                TxRemoteId = txRemoteId,
                Amount = amount,
                Type = TransactionType.Stake
            });

            result.IsSucces.Should().BeFalse();
            wallet.Balance.Should().Be(100);
        }

        [Fact]
        public async Task ProcessTransactionAsync_ShouldRejectTransaction_WhenWalletDoesNotExist()
        {
            // Arrange
            var playerId = Guid.NewGuid();
            var txRemoteId = "tx127";
            _walletRepositoryMock.Setup(r => r.GetWalletAsync(playerId)).ReturnsAsync((Wallet)null);

            // Act
            var result = await _transactionService.ProcessTransactionAsync(new TransactionRequest
            {
                PlayerId = playerId,
                TxRemoteId = txRemoteId,
                Amount = 50m,
                Type = TransactionType.Withdraw
            });

            // Assert
            result.IsSucces.Should().BeFalse();
        }
    }

    public class WalletServiceTests
    {
        private readonly Mock<IWalletRepository> _walletRepositoryMock = new Mock<IWalletRepository>();
        private readonly Mock<ITransactionService> _transactionServiceMock = new Mock<ITransactionService>();
        private readonly IWalletService _walletService;

        public WalletServiceTests()
        {
            _walletService = new WalletService(
                _transactionServiceMock.Object,
                _walletRepositoryMock.Object,
                new NullLogger<WalletService>()
            );
        }

        [Fact]
        public async Task CreateWalletAsync_ShouldCreateWallet_WhenNotExists()
        {
            var playerId = Guid.NewGuid();
            _walletRepositoryMock.Setup(r => r.GetWalletAsync(playerId)).ReturnsAsync((Wallet)null);
            _walletRepositoryMock.Setup(r => r.CreateWalletAsync(It.IsAny<Wallet>())).ReturnsAsync(true);

            var result = await _walletService.CreateWalletAsync(playerId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetBalanceAsync_ShouldReturnBalance_WhenWalletExists()
        {
            var playerId = Guid.NewGuid();
            _walletRepositoryMock.Setup(r => r.GetWalletAsync(playerId)).ReturnsAsync(new Wallet { PlayerId = playerId, Balance = 200m });

            var balance = await _walletService.GetBalanceAsync(playerId);

            balance.Should().Be(200m);
        }

        [Fact]
        public async Task DepositAsync_ShouldIncreaseBalance_WhenTransactionSuccessful()
        {
            // Arrange
            var playerId = Guid.NewGuid();
            var txRemoteId = "tx128";
            var request = new TransactionRequest { PlayerId = playerId, TxRemoteId = txRemoteId, Amount = 150m, Type = TransactionType.Deposit };
            var transactionResponse = new TransactionResponse { IsSucces = true, TxRemoteId = request.TxRemoteId,Amount = request.Amount };
            _transactionServiceMock.Setup(s => s.ProcessTransactionAsync(request)).ReturnsAsync(transactionResponse);

            // Act
            var result = await _walletService.DepositAsync(request);

            // Assert
            result.IsSucces.Should().BeTrue();
        }

        [Fact]
        public async Task WithdrawAsync_ShouldReject_WhenInsufficientFunds()
        {
            // Arrange
            var playerId = Guid.NewGuid();
            var txRemoteId = "tx129";
            var request = new TransactionRequest { PlayerId = playerId, TxRemoteId = txRemoteId, Amount = 300m, Type = TransactionType.Withdraw };
            var transactionResponse = new TransactionResponse { IsSucces = false, TxRemoteId = request.TxRemoteId };
            _transactionServiceMock.Setup(s => s.ProcessTransactionAsync(request)).ReturnsAsync(transactionResponse);

            // Act
            var result = await _walletService.WithdrawAsync(request);

            // Assert
            result.IsSucces.Should().BeFalse();
        }
    }

}