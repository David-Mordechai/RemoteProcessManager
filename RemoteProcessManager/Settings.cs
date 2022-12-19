using RemoteProcessManager.Enums;

namespace RemoteProcessManager;

public class Settings
{
    public ModeType AgentMode { get; set; } = ModeType.Agent;
    public string AgentName { get; set; } = default!;
    public string MessageBrokerUrl { get; set; } = default!;
    public int HttpPort { get; set; } = 80;
    public string ProcessFullName { get; set; } = default!;
    public string ProcessArguments { get; set; } = default!;
}