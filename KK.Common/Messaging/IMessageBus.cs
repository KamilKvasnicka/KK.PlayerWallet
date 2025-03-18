namespace KK.Common.Messaging
{
    public interface IMessageBus
    {
        Task PublishAsync(string queue, object message);
        Task SubscribeAsync<T>(string queue, Func<T, Task> handler);
    }
}
