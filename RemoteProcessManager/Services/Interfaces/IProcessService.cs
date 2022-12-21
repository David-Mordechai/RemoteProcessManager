using System.Diagnostics;
using RemoteProcessManager.Models;

namespace RemoteProcessManager.Services.Interfaces;

public interface IProcessService
{
    void StartProcess(RemoteProcessModel processModel, Action<string> streamLogsAction);
    void StopProcess();
    Process? GetRunningProcess(int? processId);
    void AttachToProcess(RemoteProcessModel processModel, Process process, Action<string> streamLogsAction);
    event EventHandler<RemoteProcessModel>? OnRestartProcess;

}