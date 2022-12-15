namespace RemoteProcessManager.MessageBroker;

public interface IProducer : IDisposable
{
    void ProduceAsync(string topic, string message, CancellationToken cancellationToken);
}