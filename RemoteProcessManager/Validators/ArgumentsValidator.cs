using CommandLine;
using RemoteProcessManager.Enums;
using RemoteProcessManager.Models;

namespace RemoteProcessManager.Validators;

internal static class ArgumentsValidator
{
    public static (bool invalid, string errorMessage) Validate(ParserResult<Settings> parserResult)
    {
        if (parserResult.Errors.Any()) return (invalid: true, errorMessage: "Required arguments was not provided.");

        if (parserResult.Value.AgentMode is not ModeType.AgentProxy) return (invalid: false, string.Empty);

        if (string.IsNullOrEmpty(parserResult.Value.ProcessFullName))
            return (invalid: true, "AgentProxy must have a process-name argument");

        if (string.IsNullOrEmpty(parserResult.Value.ProcessArguments)) return (invalid: false, string.Empty);

        if (parserResult.Value.ProcessArguments.StartsWith("\"") is false ||
            parserResult.Value.ProcessArguments.EndsWith("\"") is false)
            return (invalid:true,
                "AgentProxy must have a process-args argument within a string, example: \"\\\"-n david -a a1\\\"\"");

        parserResult.Value.ProcessArguments = parserResult.Value.ProcessArguments.Replace("\"", "");

        return (invalid: false, string.Empty);
    }
}