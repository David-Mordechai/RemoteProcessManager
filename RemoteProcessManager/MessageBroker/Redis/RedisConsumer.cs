using RemoteProcessManager.Models;
using StackExchange.Redis;

namespace RemoteProcessManager.MessageBroker.Redis;

internal class RedisConsumer : IConsumer
{
    private readonly ILogger<RedisConsumer> _logger;
    private ISubscriber? _consumer;
    private readonly ConnectionMultiplexer? _redis;

    public RedisConsumer(ILogger<RedisConsumer> logger, Settings settings)
    {
        _logger = logger;
        _redis = ConnectionMultiplexer.Connect(settings.MessageBrokerUrl);
        _consumer = _redis.GetSubscriber();

        if (_consumer is null)
        {
            throw new Exception("Fail to subscribe to Redis");
        }
    }

    public void Subscribe(string topic, Action<string> consumeMessageHandler,
        CancellationToken cancellationToken)
    {
        cancellationToken.Register(Dispose);
        try
        {
            _consumer?.Subscribe(topic, (_, message) =>
            {
                if (string.IsNullOrEmpty(message) is false)
                    consumeMessageHandler.Invoke(message!);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RedisConsumer failed, {Error}", ex.Message);
        }
    }

    public void Dispose()
    {
        if (_consumer is null) return;
        _logger.LogInformation("Redis consumer disposed...");
        _consumer?.UnsubscribeAll(CommandFlags.FireAndForget);
        _consumer?.Multiplexer.Close();
        _consumer = null;
        _redis?.Dispose();
    }
}