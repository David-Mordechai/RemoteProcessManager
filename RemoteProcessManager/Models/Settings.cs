using CommandLine;
using RemoteProcessManager.Enums;

namespace RemoteProcessManager.Models;

public class Settings
{
    [Option('m', "agent-mode", Required = true, HelpText = "Set Agent mode, 1 = Agent, 2 = ProxyAgent")]
    public ModeType AgentMode { get; set; } = ModeType.Agent;

    [Option('n', "agent-name", Required = true,
        HelpText = "Set Agent Name, for example video1, this name will be use for creating message broker topics")]
    public string AgentName { get; set; } = default!;

    [Option('b', "messageBroker-url", Required = true, HelpText = "Set MessageBroker Url, for example, redis url 127.0.0.1:6379")]
    public string MessageBrokerUrl { get; set; } = default!;

    [Option('h', "http-port", Required = true, HelpText = "Set Http Port, port for rest api")]
    public ulong HttpPort { get; set; } = default!;

    [Option('p', "process-name", HelpText = "Set remote process fullname. Only set if Agent Mode is ProxyAgent")]
    public string ProcessFullName { get; set; } = default!;

    [Option('a', "process-args", HelpText = "Set remote process arguments. Only set if Agent Mode is ProxyAgent")]
    public string ProcessArguments { get; set; } = default!;

    public string StartProcessTopic => $"start_process_{AgentName}";
    public string StreamLogsTopic => $"stream_logs_{AgentName}";
    public string StopProcessTopic => $"stop_process_{AgentName}";
}