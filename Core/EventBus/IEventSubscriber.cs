namespace Core.EventBus;

public interface IEventSubscriber : IAsyncDisposable
{
    Task SubscribeAsync(string queueName, string routingKey, Func<string, Task> handler,
        CancellationToken ct = default);
}