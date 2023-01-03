using CommandLine;
using RemoteProcessManager.Enums;
using RemoteProcessManager.Models;

namespace RemoteProcessManager.Validators;

internal static class ArgumentsValidator
{
    public static bool Validate(ParserResult<Settings> parserResult)
    {
        if (parserResult.Errors.Any())
        {
            ConsoleWriteError("Required arguments was not provided.");
            return true;
        }

        if (parserResult.Value.AgentMode is not ModeType.AgentProxy) return false;

        if (string.IsNullOrEmpty(parserResult.Value.ProcessFullName) is false) return false;
        ConsoleWriteError("AgentProxy must have a process-name argument.");
        
        return true;
    }

    private static void ConsoleWriteError(string errorMessage)
    {
        var defaultColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(errorMessage);
        Console.ForegroundColor = defaultColor;
    }
}