using StackExchange.Redis;

namespace RemoteProcessManager.MessageBroker.Redis;

internal class RedisConsumer : IConsumer
{
    private readonly ISubscriber _consumer;
    private readonly ConnectionMultiplexer _redis;

    public RedisConsumer(Settings settings)
    {
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
        try
        {
            _consumer.Subscribe(topic, (_, message) =>
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException();

                if (string.IsNullOrEmpty(message) is false)
                    consumeMessageHandler.Invoke(message!);
            });
        }
        catch (Exception ex)
        {
            throw ex switch
            {
                OperationCanceledException => new Exception("Operation was canceled."),
                _ => new Exception(ex.Message)
            };
        }
    }

    public void Dispose()
    {
        _redis.Dispose();
        GC.SuppressFinalize(this);
    }
}