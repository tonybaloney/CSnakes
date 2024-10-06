using CSnakes.Runtime.Locators;
using Microsoft.Extensions.Logging;
using System.Diagnostics;


namespace CSnakes.Runtime.EnvironmentManagement;
internal class CondaEnvironmentManagement : IEnvironmentManagement
{
    private readonly string name;
    private readonly bool ensureExists;
    private CondaLocator conda;

    internal CondaEnvironmentManagement(string name, bool ensureExists, CondaLocator conda)
    {
        this.name = name;
        this.conda = conda;
        this.ensureExists = ensureExists;
    }

    public void EnsureEnvironment(ILogger logger, PythonLocationMetadata pythonLocation)
    {
        if (!ensureExists)
            return;


        var fullPath = Path.GetFullPath(GetPath());
        if (!Directory.Exists(fullPath))
        {
            logger.LogInformation("Creating conda environment at {fullPath} using {PythonBinaryPath}", fullPath, pythonLocation.PythonBinaryPath);
            using Process process1 = ProcessUtils.ExecutePythonCommand(logger, pythonLocation, $"-VV");
            using Process process2 = ProcessUtils.ExecutePythonCommand(logger, pythonLocation, $"-m conda env create -n {name}");
        }
        else
        {
            logger.LogDebug("Conda environment already exists at {fullPath}", fullPath);
        }
    }

    public string GetPath()
    {
        return Path.Combine(conda.CondaHome, "envs", name);
    }
}
