﻿using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using System.Diagnostics;

namespace CSnakes.Runtime.Locators;

internal class CondaLocator : PythonLocator
{
    private readonly string folder;
    private readonly Version version;
    private readonly ILogger logger;
    private readonly string condaBinaryPath;

    protected override Version Version { get { return version; } }

    internal CondaLocator(ILogger logger, string condaBinaryPath)
    {
        this.logger = logger;
        this.condaBinaryPath = condaBinaryPath;
        var (process, result, errors) = ExecuteCondaCommand($"info --json");
        if (process.ExitCode != 0)
        {
            logger.LogError("Failed to determine Python version from Conda {Error}.", errors);
            throw new InvalidOperationException("Could not determine Python version from Conda.");
        }
        process.Dispose();
        // Parse JSON output to get the version
        var json = JsonNode.Parse(result ?? "")!;
        var versionAttribute = json["python_version"]?.GetValue<string>() ?? string.Empty;

        if (string.IsNullOrEmpty(versionAttribute))
        {
            throw new InvalidOperationException("Could not determine Python version from Conda.");
        }

        var basePrefix = json["root_prefix"]?.GetValue<string>() ?? string.Empty;
        if (string.IsNullOrEmpty(basePrefix))
        {
            throw new InvalidOperationException("Could not determine Conda home.");
        }

        version = ServiceCollectionExtensions.ParsePythonVersion(versionAttribute);
        folder = basePrefix;
    }

    internal (Process process, string? output, string? errors) ExecuteCondaCommand(string arguments) => ProcessUtils.ExecuteCommand(logger, condaBinaryPath, arguments);

    internal bool ExecuteCondaShellCommand(string arguments) => ProcessUtils.ExecuteShellCommand(logger, condaBinaryPath, arguments);

    public override PythonLocationMetadata LocatePython() =>
        LocatePythonInternal(folder);

    public string CondaHome { get { return folder; } }
}
