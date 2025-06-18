using CSnakes.Runtime.Locators;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.EnvironmentManagement;
#pragma warning disable CS9113 // Parameter is unread. There for future use.
internal class CondaEnvironmentManagement(ILogger? logger, string name, bool ensureEnvironment, CondaLocator conda, string? environmentSpecPath, string? pythonEnvironmentVersion) : IEnvironmentManagement
#pragma warning restore CS9113 // Parameter is unread.
{
    ILogger? IEnvironmentManagement.Logger => logger;

    public void EnsureEnvironment(PythonLocationMetadata pythonLocation)
    {
        if (!ensureEnvironment)
            return;

        var fullPath = Path.GetFullPath(GetPath());
        if (!Directory.Exists(fullPath))
        {
            logger?.LogError("Cannot find conda environment at {fullPath}.", fullPath);

            throw new InvalidOperationException($"Cannot find conda environment at {fullPath}.");

            // TODO: Automate the creation of the conda environments. 
            //var result = conda.ExecuteCondaShellCommand($"env create -n {name} -f {environmentSpecPath}");
            //if (!result)
            //{
            //    logger?.LogError("Failed to create conda environment.");
            //    throw new InvalidOperationException("Could not create conda environment");
            //}
        }
        else
        {
            logger?.LogDebug("Conda environment already exists at {fullPath}", fullPath);
            // TODO: Check if the environment is up to date
        }
    }

    public string GetPath()
    {
        // TODO: Conda environments are not always in the same location. Resolve the path correctly. 
        return Path.Combine(conda.CondaHome, "envs", name);
    }

    public virtual string GetExtraPackagePath(PythonLocationMetadata location)
    {
        var envLibPath = string.Empty;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            envLibPath = Path.Combine(GetPath(), "Lib", "site-packages");
        else
        {
            Version version = string.IsNullOrEmpty(pythonEnvironmentVersion) ? location.Version : ServiceCollectionExtensions.ParsePythonVersion(pythonEnvironmentVersion);
            string suffix = location.FreeThreaded ? "t" : "";
            // If the environment has a different version to the version in conda.
            envLibPath = Path.Combine(GetPath(), "lib", $"python{version.Major}.{version.Minor}{suffix}", "site-packages");
        }
        return envLibPath;
    }
}
