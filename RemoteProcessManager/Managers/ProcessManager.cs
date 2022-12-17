using System.Diagnostics;

namespace RemoteProcessManager.Managers;

internal class ProcessManager : IProcessManager
{
    private readonly ILogger<ProcessManager> _logger;
    private Process? _process;

    public ProcessManager(ILogger<ProcessManager> logger)
    {
        _logger = logger;
    }

    public void StartProcess(string processFullName, Action<string> onOutputData)
    {
        if (_process?.HasExited is false)
        {
            _logger.LogWarning("Closing old process - ProcessId {ProcessId}", _process.Id);
            _process.CloseMainWindow();
            _process.Close();
            _process.Dispose();
        }

        _logger.LogInformation("Starting process - {ProcessFullName}", processFullName);
        _process = new Process();
        _process.StartInfo.FileName = processFullName;
        _process.StartInfo.UseShellExecute = false;
        _process.StartInfo.RedirectStandardOutput = true;
        _process.OutputDataReceived += (_, e) =>
        {
            if (string.IsNullOrEmpty(e.Data)) return;
            onOutputData.Invoke(e.Data);
        };

        _process.Start();
        _logger.LogInformation("Process started - ProcessId {ProcessId}", _process.Id);

        // Asynchronously read the standard output of the spawned process.
        // This raises OutputDataReceived events for each line of output.
        _process.BeginOutputReadLine();
        _process.WaitForExit();
    }
}