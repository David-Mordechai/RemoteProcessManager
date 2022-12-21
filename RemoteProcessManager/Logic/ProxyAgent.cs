using System.Text.Json;
using RemoteProcessManager.Logic.Interfaces;
using RemoteProcessManager.MessageBroker;
using RemoteProcessManager.Models;

namespace RemoteProcessManager.Logic;

internal class ProxyAgent : IAgent
{
    private readonly Settings _settings;
    private readonly IConsumer _consumer;
    private readonly IProducer _producer;

    public ProxyAgent(Settings settings,
        IConsumer consumer, IProducer producer)
    {
        _settings = settings;
        _consumer = consumer;
        _producer = producer;
    }

    public void Start(CancellationToken cancellationToken)
    {
        Console.CancelKeyPress += (_, _) => StopProcess();
        if (cancellationToken.IsCancellationRequested)
            StopProcess();

        _consumer.Subscribe(_settings.StreamLogsTopic, Console.WriteLine, cancellationToken);

        _producer.Produce(_settings.StartProcessTopic, JsonSerializer.Serialize(new RemoteProcessModel
        {
            FullName = _settings.ProcessFullName,
            Arguments = _settings.ProcessArguments
        }));
    }

    private void StopProcess() =>
        _producer.Produce(_settings.StopProcessTopic, "");
}