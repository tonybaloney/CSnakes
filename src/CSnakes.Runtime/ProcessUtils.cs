using CSnakes.Runtime.Locators;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CSnakes.Runtime;

internal static class ProcessUtils
{
    internal static (Process proc, string? result, string? errors) ExecutePythonCommand(ILogger logger, PythonLocationMetadata pythonLocation, string arguments)
    {
        ProcessStartInfo startInfo = new()
        {
            WorkingDirectory = pythonLocation.Folder,
            FileName = pythonLocation.PythonBinaryPath,
            Arguments = arguments,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        return ExecuteCommand(logger, startInfo);
    }

    internal static (Process proc, string? result, string? errors) ExecuteCommand(ILogger logger, string fileName, string arguments)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        return ExecuteCommand(logger, startInfo);
    }

    internal static bool ExecuteShellCommand(ILogger logger, string fileName, string arguments)
    {
        logger.LogDebug("Executing shell command {FileName} {Arguments}", fileName, arguments);
        ProcessStartInfo startInfo = new()
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = true,
        };
        Process process = new() { StartInfo = startInfo };
        process.Start();
        process.WaitForExit();
        return process.ExitCode == 0;
    }


    private static (Process proc, string? result, string? errors) ExecuteCommand(ILogger logger, ProcessStartInfo startInfo) { 
        Process process = new() { StartInfo = startInfo };
        string? result = null;
        string? errors = null;
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                result += e.Data;
                logger.LogDebug("{Data}", e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                errors += e.Data;
                logger.LogError("{Data}", e.Data);
            }
        };

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        process.WaitForExit();
        return (process, result, errors);
    }
}
