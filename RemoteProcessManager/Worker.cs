using System.Text.Json;
using RemoteProcessManager.Enums;
using RemoteProcessManager.Managers.Interfaces;
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
    private readonly ICacheManager _cacheManager;
    private readonly string _startProcessTopic;
    private readonly string _streamLogsTopic;
    private readonly string _stopProcessTopic;

    public Worker(ILogger<Worker> logger, Settings settings, 
        IConsumer consumer, IProducer producer, IProcessManager processManager, ICacheManager cacheManager)
    {
        _logger = logger;
        _settings = settings;
        _consumer = consumer;
        _producer = producer;
        _processManager = processManager;
        _cacheManager = cacheManager;
        _startProcessTopic = $"start_process_{_settings.AgentName}";
        _streamLogsTopic = $"stream_logs_{_settings.AgentName}";
        _stopProcessTopic = $"stop_process_{_settings.AgentName}";
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker started as {AgentMode}...", _settings.AgentMode.ToString("G"));

        if (_settings.AgentMode == ModeType.Agent)
        {
            var cachedRemoteProcessModel = _cacheManager.Get(_settings.AgentName);
            if (cachedRemoteProcessModel is not null)
                StartOrAttachProcess(cachedRemoteProcessModel);

            _consumer.Subscribe(_startProcessTopic,
                processModelJson =>
                {
                    var processModel = JsonSerializer.Deserialize<RemoteProcessModel>(processModelJson);
                    if (processModel is null) return;
                    StartOrAttachProcess(processModel);
                }, cancellationToken);

            _consumer.Subscribe(_stopProcessTopic, _ => _processManager.StopProcess(), cancellationToken);

            _processManager.OnRestartProcess += (_, processModel) =>
            {
                _processManager.StartProcess(processModel,
                    outputData => _producer.Produce(_streamLogsTopic, outputData));
            };
        }

        if (_settings.AgentMode == ModeType.AgentProxy)
        {
            Console.CancelKeyPress += (_, _) => StopProcess();
            if (cancellationToken.IsCancellationRequested)
                StopProcess();

            _consumer.Subscribe(_streamLogsTopic, Console.WriteLine, cancellationToken);

            _producer.Produce(_startProcessTopic, JsonSerializer.Serialize(new RemoteProcessModel
            {
                FullName = _settings.ProcessFullName,
                Arguments = _settings.ProcessArguments
            }));
        }

        return Task.CompletedTask;
    }

    private void StartOrAttachProcess(RemoteProcessModel processModel)
    {
        var runningProcess = _processManager.GetRunningProcess(processModel.ProcessId);
        
        if (runningProcess is not null)
            _processManager.AttachToProcess(processModel, runningProcess,
                outputData => _producer.Produce(_streamLogsTopic, outputData));
        else
            _processManager.StartProcess(processModel,
                outputData => _producer.Produce(_streamLogsTopic, outputData));
    }

    private void StopProcess() => 
        _producer.Produce(_stopProcessTopic, "");
}