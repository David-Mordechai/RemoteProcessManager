namespace RemoteProcessManager.MessageBroker;

public interface IConsumer : IDisposable
{
    void Subscribe(string topic, Action<string> consumeMessageHandler,
        CancellationToken cancellationToken);
}