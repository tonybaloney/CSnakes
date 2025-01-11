using CSnakes.Runtime.Locators;
using Microsoft.Extensions.Logging;

namespace CSnakes.Runtime.EnvironmentManagement;
internal class VenvEnvironmentManagement(ILogger logger, string path, bool ensureExists) : IEnvironmentManagement
{
    ILogger IEnvironmentManagement.Logger => logger;

    public void EnsureEnvironment(PythonLocationMetadata pythonLocation)
    {
        if (!ensureExists)
            return;

        if (string.IsNullOrEmpty(path))
        {
            logger.LogError("Virtual environment location is not set but it was requested to be created.");
            throw new ArgumentNullException(nameof(path), "Virtual environment location is not set.");
        }
        var fullPath = Path.GetFullPath(path);
        if (!Directory.Exists(path))
        {
            logger.LogDebug("Creating virtual environment at {VirtualEnvPath} using {PythonBinaryPath}", fullPath, pythonLocation.PythonBinaryPath);
            var (process1, _, _) = ProcessUtils.ExecutePythonCommand(logger, pythonLocation, $"-VV");
            var (process2, _, error) = ProcessUtils.ExecutePythonCommand(logger, pythonLocation, $"-m venv {fullPath}");

            if (process1.ExitCode != 0 || process2.ExitCode != 0)
            {
                logger.LogError("Failed to create virtual environment.");
                process1.Dispose();
                process2.Dispose();
                throw new InvalidOperationException($"Could not create virtual environment. {error}");
            }
            process1.Dispose();
            process2.Dispose();
        }
        else
        {
            logger.LogDebug("Virtual environment already exists at {VirtualEnvPath}", fullPath);
        }
    }

    public string GetPath()
    {
        return path;
    }
}
