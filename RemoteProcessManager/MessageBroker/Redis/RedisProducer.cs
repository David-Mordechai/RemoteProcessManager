using RemoteProcessManager.Models;
using StackExchange.Redis;

namespace RemoteProcessManager.MessageBroker.Redis;

internal class RedisProducer : IProducer
{
    private readonly ISubscriber _producer;
    private readonly ConnectionMultiplexer _redis;

    public RedisProducer(Settings settings)
    {
        _redis = ConnectionMultiplexer.Connect(settings.MessageBrokerUrl);
        _producer = _redis.GetSubscriber();

        if (_producer is null)
        {
            throw new Exception("Fail to subscribe to Redis");
        }
    }

    public void Produce(string topic, string message)
    {
        try
        {
            _producer.Publish(topic, message);
        }
        catch (Exception e)
        {
            throw e switch
            {
                _ => new Exception(e.Message)
            };
        }
    }

    public void Dispose()
    {
        _redis.Dispose();
        GC.SuppressFinalize(this);
    }
}