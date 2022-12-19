namespace RemoteProcessManager.Managers;

public interface IProcessManager
{
    void StartProcess(string processFullName, string processArguments, Action<string> streamLogsAction);
    void StopProcess();
}