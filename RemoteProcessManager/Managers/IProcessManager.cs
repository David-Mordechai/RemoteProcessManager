namespace RemoteProcessManager.Managers;

public interface IProcessManager
{
    void StartProcess(string processFullName, Action<string> onOutputData);
}