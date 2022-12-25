using System.Diagnostics;
using RemoteProcessManager.Models;
using RemoteProcessManager.Services.Interfaces;

namespace RemoteProcessManager.Services;

internal class ProcessService : IProcessService
{
    private readonly ILogger<ProcessService> _logger;
    private readonly Settings _settings;
    private readonly ICacheService<RemoteProcessModel> _cacheService;

    public event EventHandler<RemoteProcessModel>? OnRestartProcess;

    public ProcessService(ILogger<ProcessService> logger, Settings settings, ICacheService<RemoteProcessModel> cacheService)
    {
        _logger = logger;
        _settings = settings;
        _cacheService = cacheService;
    }

    public void StartProcess(RemoteProcessModel processModel, Action<string> streamLogsAction)
    {
        if (File.Exists(processModel.FullName) is false)
        {
            _logger.LogWarning("Process file {processFullName}, not found.", processModel.FullName);
            streamLogsAction.Invoke($"Process file {processModel.FullName}, not found.");
            return;
        }
        Task.Factory.StartNew(() =>
        {
            try
            {
                StopProcess();
                _logger.LogInformation("Starting process - {ProcessFullName}", processModel.FullName);
                streamLogsAction.Invoke($"Starting process - {processModel.FullName}");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = processModel.FullName,
                        Arguments = processModel.Arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                AttachEventsToProcess(process, streamLogsAction);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                processModel.ProcessId = process.Id;
                _cacheService.Save(_settings.AgentName, processModel);

                _logger.LogInformation("Process started - ProcessId {ProcessId}", process.Id);
                streamLogsAction.Invoke($"Process started - ProcessId {process.Id}");

                // Wait for the process to exit and release resources
                process.WaitForExit();
                process.Close();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Start process Failed - {ProcessFullName}", processModel.FullName);
                streamLogsAction.Invoke($"Start process Failed - {processModel.FullName}, Error: {e.Message}");
            }
        });
    }

    public void AttachToProcess(RemoteProcessModel processModel, Process process,
        Action<string> streamLogsAction)
    {
        Task.Factory.StartNew(() =>
        {
            try
            {
                _logger.LogInformation("Attaching to running process - {ProcessId}", process.Id);
                streamLogsAction.Invoke($"Attaching to running process - {process.Id}");
                AttachEventsToProcess(process, streamLogsAction);

                process.EnableRaisingEvents = true;

                // Wait for the process to exit and release resources
                process.WaitForExit();
                process.Close();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Fail to attach to running process - {ProcessId}", process.Id);
                streamLogsAction.Invoke(
                    $"Fail to attach to running process - {process.Id}, Error: {e.Message}");
            }
        });
    }

    private void AttachEventsToProcess(Process? process, Action<string> streamLogsAction)
    {
        if (process is null) return;

        process.OutputDataReceived += (_, e) =>
        {
            if (string.IsNullOrEmpty(e.Data)) return;
            streamLogsAction.Invoke(e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            var cachedRemoteProcessModel = _cacheService.Get(_settings.AgentName);
            if (cachedRemoteProcessModel is null) return;
            
            _logger.LogError("Error occurred in remote process: {ErrorMessage}", e.Data);
            _logger.LogInformation("Attempt to restart process...");
            streamLogsAction.Invoke($"Error occurred in remote process: {e.Data}");
            streamLogsAction.Invoke("Attempt to restart process...");
            OnRestartProcess?.Invoke(this, cachedRemoteProcessModel);
        };
    }

    public void StopProcess()
    {
        var cachedRemoteProcessModel = _cacheService.Get(_settings.AgentName);
        if (cachedRemoteProcessModel is null) return;
        var process = GetRunningProcess(cachedRemoteProcessModel.ProcessId);
        if (process?.HasExited is not false) return;

        _logger.LogWarning("Killing process - ProcessId {ProcessId}", process.Id);
        process.Kill();
        process.Dispose();
       
        _cacheService.Delete(_settings.AgentName);
    }

    public Process? GetRunningProcess(int? processId)
    {
        processId ??= _cacheService.Get(_settings.AgentName)?.ProcessId;
        var process = Process.GetProcesses().FirstOrDefault(x => x.Id == processId);
        return process;
    }
}