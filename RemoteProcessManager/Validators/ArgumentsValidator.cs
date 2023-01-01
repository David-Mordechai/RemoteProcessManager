using CommandLine;
using RemoteProcessManager.Enums;
using RemoteProcessManager.Models;

namespace RemoteProcessManager.Validators;

internal static class ArgumentsValidator
{
    public static bool IsInvalid(ParserResult<Settings> parserResult)
    {
        if (parserResult.Errors.Any()) return true;

        if (parserResult.Value.AgentMode is not ModeType.AgentProxy) return false;

        if (string.IsNullOrEmpty(parserResult.Value.ProcessFullName))
            throw new ArgumentException("AgentProxy must have a process-name argument");

        if (string.IsNullOrEmpty(parserResult.Value.ProcessArguments)) return false;

        if (parserResult.Value.ProcessArguments.StartsWith("\"") is false ||
            parserResult.Value.ProcessArguments.EndsWith("\"") is false)
            throw new ArgumentException(
                "AgentProxy must have a process-args argument within a string, example: \"\\\"-n david -a a1\\\"\"");

        parserResult.Value.ProcessArguments = parserResult.Value.ProcessArguments.Replace("\"", "");

        return false;
    }
}