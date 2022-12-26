using System.Diagnostics;
using RemoteProcessManager.Models;

namespace RemoteProcessManager.Services.Interfaces;

public interface IProcessService
{
    void StartProcess(RemoteProcessModel processModel, Action<string> streamLogsAction);
    void StopProcess();
    Process? GetRunningProcess(int? processId);

    event EventHandler<RemoteProcessModel>? OnRestartProcess;

}