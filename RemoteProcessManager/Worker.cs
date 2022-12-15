using System.Diagnostics;
using RemoteProcessManager.Enums;
using RemoteProcessManager.MessageBroker;

namespace RemoteProcessManager;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly Settings _settings;
    private readonly IConsumer _consumer;
    private readonly IProducer _producer;
    private Process? _process;

    public Worker(ILogger<Worker> logger, Settings settings, IConsumer consumer, IProducer producer)
    {
        _logger = logger;
        _settings = settings;
        _consumer = consumer;
        _producer = producer;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Worker started as {_settings.AgentMode:G}...");

        if (_settings.AgentMode == ModeType.Agent)
            AgentLogic(stoppingToken);
        if (_settings.AgentMode == ModeType.AgentProxy)
            AgentProxyLogic(stoppingToken);
        
        return Task.CompletedTask;
    }

    private void AgentProxyLogic(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(_settings.StreamTopic, logLine =>
        {
            _logger.LogInformation(logLine);
        }, cancellationToken);
        
        
        _producer.ProduceAsync(_settings.ProcessTopic, _settings.ProcessFullName, cancellationToken);
    }

    private void AgentLogic(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(_settings.ProcessTopic, processFullName =>
        {
            if (_process?.HasExited is false)
            {
                _logger.LogWarning("Closing old process {ProcessId}", _process.Id);
                _process.CloseMainWindow();
                _process.Close();
                _process.Dispose();
            }
            
            _logger.LogInformation("Starting process - ProcessId {ProcessFullName}", processFullName);
            
            _process = new Process();
            _process.StartInfo.FileName = processFullName;
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.OutputDataReceived += (_, e) =>
            {
                if (string.IsNullOrEmpty(e.Data)) return;
                _producer.ProduceAsync(_settings.StreamTopic, e.Data, cancellationToken);
            };

            _process.Start();
            _logger.LogInformation("Process started - ProcessId {ProcessId}", _process.Id);
            // Asynchronously read the standard output of the spawned process.
            // This raises OutputDataReceived events for each line of output.
            _process.BeginOutputReadLine();
            _process.WaitForExit();
            
        }, cancellationToken);
    }
}