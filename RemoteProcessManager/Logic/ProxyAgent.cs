using System.Text.Json;
using RemoteProcessManager.Logic.Interfaces;
using RemoteProcessManager.MessageBroker;
using RemoteProcessManager.Models;

namespace RemoteProcessManager.Logic;

internal class ProxyAgent : IAgent
{
    private readonly ILogger<ProxyAgent> _logger;
    private readonly Settings _settings;
    private readonly IConsumer _consumer;
    private readonly IProducer _producer;
    private readonly IHostApplicationLifetime _lifetime;

    public ProxyAgent(ILogger<ProxyAgent> logger, Settings settings,
        IConsumer consumer, IProducer producer, IHostApplicationLifetime lifetime)
    {
        _logger = logger;
        _settings = settings;
        _consumer = consumer;
        _producer = producer;
        _lifetime = lifetime;
    }

    public void Start(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => _producer.Produce(_settings.StopProcessTopic, "stop", cancellationToken));

        var messageDelivered = _producer.Produce(_settings.StartProcessTopic, JsonSerializer.Serialize(new RemoteProcessModel
        {
            FullName = _settings.ProcessFullName,
            Arguments = _settings.ProcessArguments
        }), cancellationToken);

        if (messageDelivered is false)
        {
            _logger.LogError("Destination Agent for {AgentName}, not running..." , _settings.AgentName);
            _lifetime.StopApplication();
        }

        _consumer.Subscribe(_settings.StreamLogsTopic, Console.WriteLine, cancellationToken);
    }
}