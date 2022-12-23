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
    private readonly IHostApplicationLifetime _lifetime;

    public ProxyAgent(Settings settings,
        IConsumer consumer, IProducer producer, IHostApplicationLifetime lifetime)
    {
        _settings = settings;
        _consumer = consumer;
        _producer = producer;
        _lifetime = lifetime;
    }

    public void Start(CancellationToken cancellationToken)
    {
        Console.CancelKeyPress += (_, _) => { _lifetime.StopApplication(); };
        cancellationToken.Register(() => _producer.Produce(_settings.StopProcessTopic, "stop", cancellationToken));

        _producer.Produce(_settings.StartProcessTopic, JsonSerializer.Serialize(new RemoteProcessModel
        {
            FullName = _settings.ProcessFullName,
            Arguments = _settings.ProcessArguments
        }), cancellationToken);

        _consumer.Subscribe(_settings.StreamLogsTopic, Console.WriteLine, cancellationToken);
    }
}