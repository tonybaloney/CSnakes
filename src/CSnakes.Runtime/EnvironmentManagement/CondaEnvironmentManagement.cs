using CSnakes.Runtime.Locators;
using Microsoft.Extensions.Logging;

namespace CSnakes.Runtime.EnvironmentManagement;
internal class CondaEnvironmentManagement(string name, bool ensureExists, CondaLocator conda, string environmentSpecPath) : IEnvironmentManagement
{
    public void EnsureEnvironment(ILogger logger, PythonLocationMetadata pythonLocation)
    {
        if (!ensureExists)
            return;


        var fullPath = Path.GetFullPath(GetPath());
        if (!Directory.Exists(fullPath))
        {
            logger.LogInformation("Creating conda environment at {fullPath} using {PythonBinaryPath}", fullPath, pythonLocation.PythonBinaryPath);
            // TODO: Shell escape the name
            var result = conda.ExecuteCondaShellCommand($"env create -n {name} -f {environmentSpecPath}");
            if (!result)
            {
                logger.LogError("Failed to create conda environment.");
                throw new InvalidOperationException("Could not create conda environment");
            }
        }
        else
        {
            logger.LogDebug("Conda environment already exists at {fullPath}", fullPath);
            // TODO: Check if the environment is up to date
        }
    }

    public string GetPath()
    {
        // TODO: Conda environments are not always in the same location. Resolve the path correctly. 
        return Path.Combine(conda.CondaHome, "envs", name);
    }
}
