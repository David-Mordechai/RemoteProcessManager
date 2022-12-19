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

    public void StartProcess(string processFullName, string processArguments, Action<string> streamLogsAction)
    {
        if(File.Exists(processFullName) is false)
        {
            _logger.LogWarning("Process file {processFullName}, not found.", processFullName);
            streamLogsAction.Invoke($"Process file {processFullName}, not found.");
            return;
        }

        try
        {
            if (_process is not null)
            {
                var oldProcess = Process.GetProcessById(_process.Id);
            }
            
            StopProcess();

            _logger.LogInformation("Starting process - {ProcessFullName}", processFullName);
            streamLogsAction.Invoke($"Starting process - {processFullName}");
            _process = new Process();
            _process.StartInfo.FileName = processFullName;
            _process.StartInfo.Arguments = processArguments;
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardOutput = true;
            
            _process.OutputDataReceived += (_, e) =>
            {
                if (string.IsNullOrEmpty(e.Data)) return;
                streamLogsAction.Invoke(e.Data);
            };

            //_process.ErrorDataReceived += (sender, args) =>
            //{

            //};
            
            _process.Start();
            _logger.LogInformation("Process started - ProcessId {ProcessId}", _process.Id);
            streamLogsAction.Invoke($"Process started - ProcessId {_process.Id}");

            // Asynchronously read the standard output of the spawned process.
            // This raises OutputDataReceived events for each line of output.
            //_process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            _process.WaitForExit();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Start process Failed - {ProcessFullName}", processFullName);
            streamLogsAction.Invoke($"Start process Failed - {processFullName}, Error: {e.Message}");
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

/*
 * 1. agent need to start from temp file
   2. if remote process crashes and not cancled then kill angent process for watch dog to started again
   3. after watch dog started again agent check if remote process is running than connect to it and subscribe to stream output again
 */