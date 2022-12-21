namespace RemoteProcessManager.Models;

public class RemoteProcessModel
{
    public int? ProcessId { get; set; }
    public string FullName { get; set; } = default!;
    public string Arguments { get; set; } = default!;
}