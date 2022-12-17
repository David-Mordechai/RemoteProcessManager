using RemoteProcessManager.Enums;
using RemoteProcessManager.Managers;
using RemoteProcessManager.MessageBroker;

namespace RemoteProcessManager;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly Settings _settings;
    private readonly IConsumer _consumer;
    private readonly IProducer _producer;
    private readonly IProcessManager _processManager;

    public Worker(ILogger<Worker> logger, Settings settings, IConsumer consumer, IProducer producer, IProcessManager processManager)
    {
        _logger = logger;
        _settings = settings;
        _consumer = consumer;
        _producer = producer;
        _processManager = processManager;
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker started as {AgentMode}...", _settings.AgentMode.ToString("G"));

        if (_settings.AgentMode == ModeType.Agent)
        {
            _consumer.Subscribe(_settings.ProcessTopic,
                processFullName =>
                {
                    _processManager.StartProcess(processFullName,
                        outputData => { _producer.Produce(_settings.StreamTopic, outputData, cancellationToken); });
                }, cancellationToken);
        }
        
        if (_settings.AgentMode == ModeType.AgentProxy)
        {
            _consumer.Subscribe(_settings.StreamTopic, logLine => { _logger.LogInformation("{LogLine}", logLine); },
                cancellationToken);

            _producer.Produce(_settings.ProcessTopic, _settings.ProcessFullName, cancellationToken);
        }
        
        return Task.CompletedTask;
    }
}