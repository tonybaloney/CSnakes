using CSnakes.Runtime.Locators;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace CSnakes.Runtime;

internal static class ProcessUtils
{
    internal static (int exitCode, string? result, string? errors) ExecutePythonCommand(ILogger? logger, PythonLocationMetadata pythonLocation, params string[] arguments)
    {

        ProcessStartInfo startInfo = new(pythonLocation.PythonBinaryPath, arguments)
        {
            WorkingDirectory = pythonLocation.Folder,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
        };
        return ExecuteCommand(logger, startInfo);
    }

    internal static (int exitCode, string? result, string? errors) ExecuteCommand(ILogger? logger, string fileName, params string[] arguments)
    {
        ProcessStartInfo startInfo = new(fileName, arguments)
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
        };
        return ExecuteCommand(logger, startInfo);
    }

    internal static bool ExecuteShellCommand(ILogger? logger, string fileName, params string[] arguments)
    {
        logger?.LogDebug("Executing shell command {FileName} {Arguments}", fileName, arguments);
        ProcessStartInfo startInfo = new(fileName, arguments)
        {
            UseShellExecute = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };
        using Process process = new() { StartInfo = startInfo };
        process.Start();
        process.WaitForExit();
        return process.ExitCode == 0;
    }


    private static (int exitCode, string? result, string? errors) ExecuteCommand(ILogger? logger, ProcessStartInfo startInfo)
    {
        using Process process = new() { StartInfo = startInfo };
        string? result = null;
        string? errors = null;
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                result += e.Data;
                logger?.LogDebug("{Data}", e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                errors += e.Data;
                logger?.LogError("{Data}", e.Data);
            }
        };

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        process.WaitForExit();
        return (process.ExitCode, result, errors);
    }

    internal static void ExecuteProcess(string fileName, IEnumerable<string> arguments, string workingDirectory, string path, ILogger? logger, IReadOnlyDictionary<string, string?>? extraEnv = null)
    {
        ProcessStartInfo startInfo = new(fileName, arguments)
        {
            WorkingDirectory = workingDirectory,
            CreateNoWindow = true,
        };

        if (!string.IsNullOrEmpty(path))
            startInfo.EnvironmentVariables["PATH"] = path;
        if (extraEnv is not null)
        {
            foreach (var kvp in extraEnv)
            {
                if (kvp.Value is not null)
                    startInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
            }
        }
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        logger?.LogDebug($"Running {startInfo.FileName} with args {startInfo.Arguments} from {startInfo.WorkingDirectory}");

        using Process process = new() { StartInfo = startInfo };
        string stderr = string.Empty;
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                logger?.LogDebug("{Data}", e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                stderr += e.Data + Environment.NewLine;
            }
        };

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            logger?.LogError("Failed to install packages. ");
            logger?.LogError("Output was: {stderr}", stderr);
            throw new InvalidOperationException("Failed to install packages");
        }
        else
        {
            logger?.LogDebug("Successfully installed packages.");
            logger?.LogDebug("Output was: {stderr}", stderr);
        }
    }
}
