﻿using System;
using System.Diagnostics;
using RemoteProcessManager.Managers.Interfaces;
using RemoteProcessManager.Models;

namespace RemoteProcessManager.Managers;

internal class ProcessManager : IProcessManager
{
    private readonly ILogger<ProcessManager> _logger;
    private readonly Settings _settings;
    private readonly ICacheManager _cacheManager;
    private RemoteProcessModel? _cachedRemoteProcessModel;
    
    public event EventHandler<RemoteProcessModel>? OnRestartProcess;

    public ProcessManager(ILogger<ProcessManager> logger, Settings settings, ICacheManager cacheManager)
    {
        _logger = logger;
        _settings = settings;
        _cacheManager = cacheManager;
        
    }

    public void StartProcess(RemoteProcessModel processModel, Action<string> streamLogsAction)
    {
        if (File.Exists(processModel.FullName) is false)
        {
            _logger.LogWarning("Process file {processFullName}, not found.", processModel.FullName);
            streamLogsAction.Invoke($"Process file {processModel.FullName}, not found.");
            return;
        }

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
            _cacheManager.Save(_settings.AgentName, processModel);
            _cachedRemoteProcessModel = processModel;

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
    }

    public void AttachToProcess(RemoteProcessModel processModel, Process process,
        Action<string> streamLogsAction)
    {
        try
        {
            _logger.LogInformation("Attaching to running process - {ProcessId}", processModel.ProcessId);
            streamLogsAction.Invoke($"Attaching to running process - {processModel.ProcessId}");
            AttachEventsToProcess(process, streamLogsAction);
            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Wait for the process to exit and release resources
            process.WaitForExit();
            process.Close();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fail to attach to running process - {ProcessId}", processModel.ProcessId);
            streamLogsAction.Invoke($"Fail to attach to running process - {processModel.ProcessId}, Error: {e.Message}");
        }
    }

    private void AttachEventsToProcess(Process? process, Action<string> streamLogsAction)
    {
        if(process is null) return;
        
        process.OutputDataReceived += (_, e) =>
        {
            if (string.IsNullOrEmpty(e.Data)) return;
            streamLogsAction.Invoke(e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            _logger.LogError("Error occurred in remote process: {ErrorMessage}", e.Data);
            _logger.LogInformation("Attempt to restart process...");
            streamLogsAction.Invoke($"Error occurred in remote process: {e.Data}");
            streamLogsAction.Invoke("Attempt to restart process...");
            if(_cachedRemoteProcessModel is not null) OnRestartProcess?.Invoke(this, _cachedRemoteProcessModel);
        };
    }

    public void StopProcess()
    {
        if(_cachedRemoteProcessModel is null) return;
        var process = GetRunningProcess(_cachedRemoteProcessModel.ProcessId);
        if (process?.HasExited is not false) return;
        _logger.LogWarning("Killing old process - ProcessId {ProcessId}", process.Id);
        _cacheManager.Delete(_settings.AgentName);
        process.Kill();
        process.WaitForExit();
        process.Dispose();
    }

    public Process? GetRunningProcess(int? processId)
    {
        var process = Process.GetProcesses().FirstOrDefault(x => x.Id == processId);
        return process;
    }
}

/*
 * 1. agent need to start from temp file
   2. if remote process crashes and not canceled then kill agent process for watch dog to started again
   3. after watch dog started again agent check if remote process is running than connect to it and subscribe to stream output again
 */