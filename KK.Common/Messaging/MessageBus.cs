
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;

namespace KK.Common.Messaging
{
    public class MessageBus : IMessageBus, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<MessageBus> _logger;

        public MessageBus(ILogger<MessageBus> logger)
        {
            _logger = logger;
            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq", // Docker service name for RabbitMQ
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _logger.LogInformation("MessageBus connected to RabbitMQ at {HostName}", factory.HostName);
        }

        /// <summary>
        /// Publishes messages to RabbitMQ
        /// </summary>
        public Task PublishAsync(string queue, object message)
        {
            _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true; // Setting for persistent message

            _channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: properties, body: body);

            _logger.LogInformation("[Sent] Message to queue '{Queue}': {Message}", queue, message);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Subscribes to messages from RabbitMQ and processes asynchronously
        /// </summary>
        public Task SubscribeAsync<T>(string queue, Func<T, Task> handler)
        {
            _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var data = JsonSerializer.Deserialize<T>(message);

                    if (data != null)
                    {
                        await handler(data);
                        _logger.LogInformation("[Processed] Message from queue '{Queue}': {Data}", queue, data);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from queue '{Queue}'", queue);
                }
            };

            _channel.BasicConsume(queue, autoAck: true, consumer: consumer);

            _logger.LogInformation("[Listening] on queue '{Queue}'", queue);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Disposes the RabbitMQ connection
        /// </summary>
        public void Dispose()
        {
            _logger.LogInformation("Disposing MessageBus and closing connections");
            _channel?.Close();
            _connection?.Close();
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
