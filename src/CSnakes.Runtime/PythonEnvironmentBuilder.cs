using CSnakes.Runtime.Locators;
using CSnakes.Runtime.PackageManagement;
using Python.Runtime;
using System.Diagnostics;

namespace CSnakes.Runtime;

internal partial class PythonEnvironmentBuilder(IEnumerable<PythonLocator> locators, IEnumerable<IPythonPackageInstaller> packageInstallers) : IPythonEnvironmentBuilder
{
    private PythonEnvironmentInternal? env;
    private bool ensureVirtualEnvironment = false;
    private string? virtualEnvironmentLocation;
    private string[] extraPaths = [];
    private string? home;

    internal static string MapVersion(string version, string sep = "")
    {
        // split on . then take the first two segments and join them without spaces
        var versionParts = version.Split('.');
        return string.Join(sep, versionParts.Take(2));
    }

    public IPythonEnvironmentBuilder WithVirtualEnvironment(string path, bool ensureVirtualEnvironment = true)
    {
        this.ensureVirtualEnvironment = ensureVirtualEnvironment;
        virtualEnvironmentLocation = path;
        extraPaths = [.. extraPaths, path, Path.Combine(virtualEnvironmentLocation, "Lib", "site-packages")];

        return this;
    }

    public IPythonEnvironmentBuilder WithHome(string home)
    {
        this.home = home;
        return this;
    }

    public IPythonEnvironment Build()
    {
        PythonLocationMetadata location = locators.Select(locator => locator.LocatePython()).FirstOrDefault(loc => loc is not null)
            ?? throw new InvalidOperationException("Python installation not found.");

        if (PythonEngine.IsInitialized && env is not null)
        {
            // Raise exception?
            return env;
        }

        home ??= Environment.CurrentDirectory;

        if (!Directory.Exists(home))
        {
            throw new DirectoryNotFoundException("Python home directory does not exist.");
        }

        if (ensureVirtualEnvironment)
        {
            EnsureVirtualEnvironment(location);
        }

        foreach (var installer in packageInstallers)
        {
            installer.InstallPackages(home, virtualEnvironmentLocation);
        }

        env = new PythonEnvironmentInternal(location, home, this, extraPaths);

        return env;
    }

    private void EnsureVirtualEnvironment(PythonLocationMetadata pythonLocation)
    {
        if (virtualEnvironmentLocation is null)
        {
            throw new InvalidOperationException("Virtual environment location is not set.");
        }

        if (!Directory.Exists(virtualEnvironmentLocation))
        {
            ProcessStartInfo startInfo = new()
            {
                WorkingDirectory = pythonLocation.Folder,
                FileName = "python",
                Arguments = $"-m venv {virtualEnvironmentLocation}"
            };
            using Process process = new() { StartInfo = startInfo };
            process.WaitForExit();
        }
    }

    internal void Destroy() => env = null;
}
