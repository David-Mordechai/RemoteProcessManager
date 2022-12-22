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

    public ProxyAgent(ILogger<ProxyAgent> logger, Settings settings,
        IConsumer consumer, IProducer producer)
    {
        _logger = logger;
        _settings = settings;
        _consumer = consumer;
        _producer = producer;
    }

    public void Start(CancellationToken cancellationToken)
    {
        Console.CancelKeyPress += (_, _) => StopProcess(cancellationToken);
        //cancellationToken.Register(() => StopProcess(cancellationToken));
        
        _producer.Produce(_settings.StartProcessTopic, JsonSerializer.Serialize(new RemoteProcessModel
        {
            FullName = _settings.ProcessFullName,
            Arguments = _settings.ProcessArguments
        }), cancellationToken);

        _consumer.Subscribe(_settings.StreamLogsTopic, Console.WriteLine, cancellationToken);
    }

    private void StopProcess(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping remote process...");
        _producer.Produce(_settings.StopProcessTopic, "stop", cancellationToken);
        Thread.Sleep(1000);
    }
}