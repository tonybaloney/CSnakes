using CSnakes.Runtime.Locators;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.EnvironmentManagement;
public interface IEnvironmentManagement
{
    ILogger Logger { get; }

    public string GetPath();
    public virtual string GetExtraPackagePath(PythonLocationMetadata location)
    {
        var envLibPath = string.Empty;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            envLibPath = Path.Combine(GetPath(), "Lib", "site-packages");
        else
        {
            string suffix = location.FreeThreaded ? "t" : "";
            envLibPath = Path.Combine(GetPath(), "lib", $"python{location.Version.Major}.{location.Version.Minor}{suffix}", "site-packages");
        }
        Logger.LogDebug("Adding environment site-packages to extra paths: {VenvLibPath}", envLibPath);
        return envLibPath;
    }
    public void EnsureEnvironment(PythonLocationMetadata pythonLocation);

}
