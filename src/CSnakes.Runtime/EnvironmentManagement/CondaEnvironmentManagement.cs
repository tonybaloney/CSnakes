using CSnakes.Runtime.Locators;
using Microsoft.Extensions.Logging;
using System.Diagnostics;


namespace CSnakes.Runtime.EnvironmentManagement;
internal class CondaEnvironmentManagement : IEnvironmentManagement
{
    private readonly string name;
    private readonly bool ensureExists;
    private readonly CondaLocator conda;

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
            // TODO: Shell escape the name
            var (process, _) = conda.ExecuteCondaCommand($"env create -n {name}");
            if (process.ExitCode != 0)
            {
                logger.LogError("Failed to create conda environment {Error}.", process.StandardError.ReadToEnd());
                throw new InvalidOperationException("Could not create conda environment.");
            }
        }
        else
        {
            logger.LogDebug("Conda environment already exists at {fullPath}", fullPath);
        }
    }

    public string GetPath()
    {
        // TODO: Conda environments are not always in the same location. Resolve the path correctly. 
        return Path.Combine(conda.CondaHome, "envs", name);
    }
}
