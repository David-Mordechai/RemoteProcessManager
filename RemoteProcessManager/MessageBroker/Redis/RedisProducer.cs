using RemoteProcessManager.Models;
using StackExchange.Redis;

namespace RemoteProcessManager.MessageBroker.Redis;

internal class RedisProducer : IProducer
{
    private readonly ILogger<RedisProducer> _logger;
    private readonly ISubscriber _producer;
    private readonly ConnectionMultiplexer? _redis;

    public RedisProducer(ILogger<RedisProducer> logger,  Settings settings)
    {
        _logger = logger;
        _redis = ConnectionMultiplexer.Connect(settings.MessageBrokerUrl);
        _producer = _redis.GetSubscriber();

        if (_producer is null)
        {
            throw new Exception("Fail to subscribe to Redis");
        }
    }

    public void Produce(string topic, string message, CancellationToken cancellationToken)
    {
        try
        {
            _producer.Publish(topic, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RedisConsumer failed, {Error}", ex?.Message);
        }
    }

    public void Dispose()
    {
        _redis?.Dispose();
    }
}