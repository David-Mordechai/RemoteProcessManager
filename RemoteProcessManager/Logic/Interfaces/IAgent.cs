namespace RemoteProcessManager.Logic.Interfaces;

public interface IAgent
{
    void Start(CancellationToken cancellationToken);
}