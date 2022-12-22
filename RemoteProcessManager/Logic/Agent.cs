using System.Text.Json;
using System.Threading;
using RemoteProcessManager.Logic.Interfaces;
using RemoteProcessManager.MessageBroker;
using RemoteProcessManager.Models;
using RemoteProcessManager.Services.Interfaces;

namespace RemoteProcessManager.Logic;

internal class Agent : IAgent
{
    private readonly ILogger<Agent> _logger;
    private readonly Settings _settings;
    private readonly IConsumer _consumer;
    private readonly IProducer _producer;
    private readonly IProcessService _processService;
    private readonly ICacheService<RemoteProcessModel> _cacheService;

    public Agent(ILogger<Agent> logger, Settings settings,
        IConsumer consumer, IProducer producer, IProcessService processService, ICacheService<RemoteProcessModel> cacheService)
    {
        _logger = logger;
        _settings = settings;
        _consumer = consumer;
        _producer = producer;
        _processService = processService;
        _cacheService = cacheService;
    }

    public void Start(CancellationToken cancellationToken)
    {
        var cachedRemoteProcessModel = _cacheService.Get(_settings.AgentName);
        if (cachedRemoteProcessModel is not null)
            StartOrAttachProcess(cachedRemoteProcessModel, cancellationToken);

        _consumer.Subscribe(_settings.StartProcessTopic,
            processModelJson =>
            {
                var processModel = JsonSerializer.Deserialize<RemoteProcessModel>(processModelJson);
                if (processModel is null) return;
                StartOrAttachProcess(processModel, cancellationToken);
            }, cancellationToken);

        _consumer.Subscribe(_settings.StopProcessTopic, message =>
        {
            _logger.LogInformation("subscribe event for stopping process");
            _processService.StopProcess();
        }, cancellationToken);

        _processService.OnRestartProcess += (_, processModel) =>
        {
            _processService.StartProcess(processModel,
                outputData => _producer.Produce(_settings.StreamLogsTopic, outputData, cancellationToken));
        };
    }

    private void StartOrAttachProcess(RemoteProcessModel processModel, CancellationToken cancellationToken)
    {
        //var runningProcess = _processService.GetRunningProcess(processModel.ProcessId);

        //if (runningProcess is not null)
        //    _processService.AttachToProcess(processModel, runningProcess,
        //        outputData => _producer.Produce(_settings.StreamLogsTopic, outputData, cancellationToken));
        //else
            _processService.StartProcess(processModel,
                outputData => _producer.Produce(_settings.StreamLogsTopic, outputData, cancellationToken));
    }
}