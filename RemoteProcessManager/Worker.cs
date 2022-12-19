using System.Text.Json;
using RemoteProcessManager.Enums;
using RemoteProcessManager.Managers;
using RemoteProcessManager.MessageBroker;
using RemoteProcessManager.Models;

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
            _consumer.Subscribe($"process_{_settings.AgentName}",
                processModelJson =>
                {
                    var processModel = JsonSerializer.Deserialize<ProcessModel>(processModelJson)!;
                    
                    _processManager.StartProcess(processModel.FullName, processModel.Arguments,
                        outputData => _producer.Produce($"stream_{_settings.AgentName}", outputData, cancellationToken));
                }, cancellationToken);

            _consumer.Subscribe($"cancel_{_settings.AgentName}",
                _ => _processManager.StopProcess(), 
                cancellationToken);
        }
        
        if (_settings.AgentMode == ModeType.AgentProxy)
        {
            Console.CancelKeyPress += (_, _) => CancelProcess(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                CancelProcess(cancellationToken);

            _consumer.Subscribe($"stream_{_settings.AgentName}", Console.WriteLine, cancellationToken);

            var processModelJson = JsonSerializer.Serialize(new ProcessModel
            {
                FullName = _settings.ProcessFullName, 
                Arguments = _settings.ProcessArguments
            });
            
            _producer.Produce($"process_{_settings.AgentName}", processModelJson, cancellationToken);
        }

        return Task.CompletedTask;
    }

    private void CancelProcess(CancellationToken cancellationToken) => 
        _producer.Produce($"cancel_{_settings.AgentName}", "cancel", cancellationToken);
}