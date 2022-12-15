namespace MessageBroker;

public class MessageBrokerSettings
{
    public string MessageBrokerUrl { get; set; } = default!;
    public string ProcessTopic { get; set; } = default!;
    public string StreamTopic { get; set; } = default!;
}