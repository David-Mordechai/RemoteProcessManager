namespace RemoteProcessManager.MessageBroker;

public interface IProducer : IDisposable
{
    bool Produce(string topic, string message, CancellationToken cancellationToken);
}