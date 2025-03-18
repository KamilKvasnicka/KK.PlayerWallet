using KK.Common.Messaging.Events;
using KK.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KK.Common.Messaging
{
    public class RabbitMqConsumer : IRabbitMqConsumer, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMessageBus _messageBus;
        private readonly ILogger<RabbitMqConsumer> _logger;

        public RabbitMqConsumer(IServiceScopeFactory scopeFactory, IMessageBus messageBus, ILogger<RabbitMqConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _messageBus = messageBus;
            _logger = logger;
        }

        /// <summary>
        /// Starts listening to RabbitMQ messages asynchronously.
        /// </summary>
        public async Task StartAsync()
        {
            _logger.LogInformation("RabbitMQ Consumer is starting...");

            await _messageBus.SubscribeAsync<WalletUpdateEvent>("wallet_updates", async transaction =>
            {
                using var scope = _scopeFactory.CreateScope();
                var transactionRepo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

                try
                {
                    if (transaction != null)
                    {
                        _logger.LogInformation("Received transaction update: Player {PlayerId}, Amount {Amount}",
                                                transaction.PlayerId, transaction.Amount);

                        bool saved = await transactionRepo.SaveTransaction(transaction.PlayerId, transaction.TxRemoteId, transaction.Amount);

                        if (saved)
                        {
                            _logger.LogInformation("Transaction saved successfully: Player {PlayerId}, Amount {Amount}",
                                                    transaction.PlayerId, transaction.Amount);
                        }
                        else
                        {
                            _logger.LogWarning("Transaction was not saved: Player {PlayerId}, Amount {Amount}",
                                                transaction.PlayerId, transaction.Amount);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message for Player {PlayerId}", transaction?.PlayerId);
                }
            });

            _logger.LogInformation("RabbitMQ Consumer is now listening for messages.");
        }

        /// <summary>
        /// Cleanup resources when disposing the consumer.
        /// </summary>
        public void Dispose()
        {
            _logger.LogInformation("RabbitMQ Consumer is shutting down.");
        }
    }
}
