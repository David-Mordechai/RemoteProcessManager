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

    public void StartProcess(string processFullName, string processArguments, Action<string> onOutputData)
    {
        if(File.Exists(processFullName) is false)
        {
            _logger.LogWarning("Process file {processFullName}, not found.", processFullName);
            onOutputData.Invoke($"Process file {processFullName}, not found.");
            return;
        }

        try
        {
            StopProcess();

            _logger.LogInformation("Starting process - {ProcessFullName}", processFullName);
            onOutputData.Invoke($"Starting process - {processFullName}");
            _process = new Process();
            _process.StartInfo.FileName = processFullName;
            _process.StartInfo.Arguments = processArguments;
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.OutputDataReceived += (_, e) =>
            {
                if (string.IsNullOrEmpty(e.Data)) return;
                onOutputData.Invoke(e.Data);
            };

            _process.Start();
            _logger.LogInformation("Process started - ProcessId {ProcessId}", _process.Id);
            onOutputData.Invoke($"Process started - ProcessId {_process.Id}");

            // Asynchronously read the standard output of the spawned process.
            // This raises OutputDataReceived events for each line of output.
            _process.BeginOutputReadLine();
            _process.WaitForExit();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Start process Failed - {ProcessFullName}", processFullName);
            onOutputData.Invoke($"Start process Failed - {processFullName}, Error: {e.Message}");
        }
    }

    public void StopProcess()
    {
        if (_process?.HasExited is not false) return;
        _logger.LogWarning("Killing old process - ProcessId {ProcessId}", _process.Id);
        _process.Kill();
        _process.WaitForExit();
        _process.Dispose();
        _process = null;
    }
}