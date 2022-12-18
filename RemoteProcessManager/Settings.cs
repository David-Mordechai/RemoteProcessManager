using RemoteProcessManager.Enums;

namespace RemoteProcessManager;

public class Settings
{
    public ModeType AgentMode { get; set; } = ModeType.Agent;
    public int HttpPort { get; set; } = 80;
    public string MessageBrokerUrl { get; set; } = default!;
    public string ProcessTopic { get; set; } = default!;
    public string StreamTopic { get; set; } = default!;
    public string ProcessFullName { get; set; } = default!;
    public string ProcessArguments { get; set; } = default!;
}