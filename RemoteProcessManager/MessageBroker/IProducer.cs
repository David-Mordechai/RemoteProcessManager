namespace RemoteProcessManager.MessageBroker;

public interface IProducer : IDisposable
{
    void Produce(string topic, string message, CancellationToken cancellationToken);
}