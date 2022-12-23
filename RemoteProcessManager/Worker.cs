using RemoteProcessManager.Logic.Interfaces;
using RemoteProcessManager.Models;

namespace RemoteProcessManager;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly Settings _settings;
    private readonly IAgent _agent;

    public Worker(ILogger<Worker> logger, Settings settings, IAgent agent)
    {
        _logger = logger;
        _settings = settings;
        _agent = agent;
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker started as {AgentMode}...", _settings.AgentMode.ToString("G"));
        _agent.Start(cancellationToken);
        return Task.CompletedTask;
    }
}