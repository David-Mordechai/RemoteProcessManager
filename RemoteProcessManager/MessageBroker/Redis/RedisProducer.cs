using RemoteProcessManager.Models;
using StackExchange.Redis;

namespace RemoteProcessManager.MessageBroker.Redis;

internal class RedisProducer : IProducer
{
    private readonly ILogger<RedisProducer> _logger;
    private ISubscriber? _producer;
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

    public bool Produce(string topic, string message, CancellationToken cancellationToken)
    {
        try
        {
            var clientsCount =_producer?.Publish(topic, message);

            if (cancellationToken.IsCancellationRequested)
                Dispose();

            return clientsCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RedisConsumer failed, {Error}", ex.Message);
            return false;
        }
    }

    public void Dispose()
    {
        if (_producer is null) return;
        _logger.LogInformation("Redis producer disposed...");
        _producer?.Multiplexer.Close();
        _producer = null;
        _redis?.Dispose();
    }
}