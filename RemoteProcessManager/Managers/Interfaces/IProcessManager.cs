using RemoteProcessManager.Models;
using System.Diagnostics;

namespace RemoteProcessManager.Managers.Interfaces;

public interface IProcessManager
{
    void StartProcess(RemoteProcessModel processModel, Action<string> streamLogsAction);
    void StopProcess();
    Process? GetRunningProcess(int? processId);
    void AttachToProcess(RemoteProcessModel processModel, Process process, Action<string> streamLogsAction);
    event EventHandler<RemoteProcessModel>? OnRestartProcess;

}