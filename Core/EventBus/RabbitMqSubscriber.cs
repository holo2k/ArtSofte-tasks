using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Core.EventBus;

public class RabbitMqSubscriber : IEventSubscriber
{
    private readonly string _exchangeName;
    private readonly string _host;
    private readonly ILogger<RabbitMqSubscriber> _logger;
    private readonly string _pass;
    private readonly int _port;
    private readonly string _user;
    private IChannel _channel = null!;
    private IConnection? _connection;

    public RabbitMqSubscriber(ILogger<RabbitMqSubscriber> logger,
        string host = "rabbit-mq", int port = 5672, string user = "guest", string pass = "guest",
        string exchangeName = "product.events")
    {
        _logger = logger;
        _host = host;
        _port = port;
        _user = user;
        _pass = pass;
        _exchangeName = exchangeName;
    }

    public async Task SubscribeAsync(string queueName, string routingKey, Func<string, Task> handler,
        CancellationToken ct = default)
    {
        if (_channel == null || _connection == null)
            await InitializeAsync(ct);

        await _channel.QueueDeclareAsync(
            queueName,
            true,
            false,
            false,
            null,
            false,
            false,
            ct
        );
        await _channel.QueueBindAsync(queueName, _exchangeName, routingKey, cancellationToken: ct);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (sender, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                await handler(body);
                await _channel.BasicAckAsync(ea.DeliveryTag, false, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling message {RoutingKey}", ea.RoutingKey);
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true, ct); // requeue
            }
        };

        await _channel.BasicConsumeAsync(queueName, false, consumer, ct);
        _logger.LogInformation("Subscribed to {Queue} ({RoutingKey})", queueName, routingKey);
    }

    public ValueTask DisposeAsync()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        return ValueTask.CompletedTask;
    }

    private async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = _host,
            Port = _port,
            UserName = _user,
            Password = _pass
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Topic, true, false,
            cancellationToken: cancellationToken);

        _logger.LogInformation("RabbitMQ subscriber initialized. Exchange: {Exchange}", _exchangeName);
    }
}