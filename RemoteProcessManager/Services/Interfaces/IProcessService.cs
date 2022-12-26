using RemoteProcessManager.Models;

namespace RemoteProcessManager.Services.Interfaces;

public interface IProcessService
{
    void StartProcess(RemoteProcessModel processModel, Action<string> streamLogsAction);
    void StopProcess();

    event EventHandler<RemoteProcessModel>? OnRestartProcess;

}