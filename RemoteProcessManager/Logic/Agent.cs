using System.Text.Json;
using RemoteProcessManager.Logic.Interfaces;
using RemoteProcessManager.MessageBroker;
using RemoteProcessManager.Models;
using RemoteProcessManager.Services.Interfaces;

namespace RemoteProcessManager.Logic;

internal class Agent : IAgent
{
    private readonly Settings _settings;
    private readonly IConsumer _consumer;
    private readonly IProducer _producer;
    private readonly IProcessService _processService;
    private readonly ICacheService<RemoteProcessModel> _cacheService;

    public Agent(Settings settings,
        IConsumer consumer, IProducer producer, IProcessService processService, ICacheService<RemoteProcessModel> cacheService)
    {
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
        {
            _processService.StartProcess(cachedRemoteProcessModel,
                outputData => _producer.Produce(_settings.StreamLogsTopic, outputData, cancellationToken));
        }

        _consumer.Subscribe(_settings.StartProcessTopic,
            processModelJson =>
            {
                var processModel = JsonSerializer.Deserialize<RemoteProcessModel>(processModelJson);
                if (processModel is null) return;
                
                _processService.StartProcess(processModel,
                    outputData => _producer.Produce(_settings.StreamLogsTopic, outputData, cancellationToken));
            }, cancellationToken);

        _consumer.Subscribe(_settings.StopProcessTopic, _ =>
        {
            _processService.StopProcess();
        }, cancellationToken);

        _processService.OnRestartProcess += (_, processModel) =>
        {
            _processService.StartProcess(processModel,
                outputData => _producer.Produce(_settings.StreamLogsTopic, outputData, cancellationToken));
        };
    }
}