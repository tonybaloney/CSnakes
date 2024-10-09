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
            var (process, result) = conda.ExecuteCondaCommand($"env create -n {name} -f {environmentSpecPath}");
            if (process.ExitCode != 0)
            {
                logger.LogError("Failed to create conda environment {Error}.", result);
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
