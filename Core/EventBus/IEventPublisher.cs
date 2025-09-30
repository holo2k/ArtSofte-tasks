namespace Core.EventBus;

public interface IEventPublisher
{
    bool IsConnected();
    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task PublishAsync(string routingKey, object payload, IDictionary<string, object>? headers = null,
        CancellationToken cancellationToken = default);

    Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default);
}