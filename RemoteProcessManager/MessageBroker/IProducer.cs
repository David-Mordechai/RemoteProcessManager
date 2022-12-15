namespace RemoteProcessManager.MessageBroker;

public interface IProducer : IDisposable
{
    Task ProduceAsync(string topic, string message, CancellationToken cancellationToken);
}