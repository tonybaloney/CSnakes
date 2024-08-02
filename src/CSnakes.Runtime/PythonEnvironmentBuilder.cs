using Python.Runtime;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime;

public partial class PythonEnvironmentBuilder
{
    private readonly string versionPath;
    private readonly string pythonLocation;
    private readonly string version;
    private PythonEnvironmentInternal? env;
    private string? virtualEnvironmentLocation;
    private string[] extraPaths = [];

    private PythonEnvironmentBuilder(string pythonLocation, string version)
    {
        versionPath = MapVersion(version);
        this.pythonLocation = pythonLocation;
        this.version = version;
    }

    internal static string MapVersion(string version, string sep = "")
    {
        // split on . then take the first two segments and join them without spaces
        var versionParts = version.Split('.');
        return string.Join(sep, versionParts.Take(2));
    }

    public PythonEnvironmentBuilder WithVirtualEnvironment(string path, bool ensureVirtualEnvironment = true)
    {
        if (ensureVirtualEnvironment)
        {
            EnsureVirtualEnvironment(path);
        }

        virtualEnvironmentLocation = path;
        extraPaths = [.. extraPaths, path, Path.Combine(virtualEnvironmentLocation, "Lib", "site-packages")];

        return this;
    }

    public IPythonEnvironment Build(string home)
    {
        if (PythonEngine.IsInitialized && env is not null)
        {
            // Raise exception?
            return env;
        }

        if (!Directory.Exists(home))
        {
            throw new DirectoryNotFoundException("Python home directory does not exist.");
        }

        if (File.Exists(Path.Combine(home, "requirements.txt")))
        {
            InstallPackagesWithPip(home);
        }

        env = new PythonEnvironmentInternal(pythonLocation, version, home, this, extraPaths);

        return env;
    }

    private void InstallPackagesWithPip(string home)
    {
        ProcessStartInfo startInfo = new()
        {
            WorkingDirectory = home,
            FileName = "pip",
            Arguments = "install -r requirements.txt"
        };

        if (virtualEnvironmentLocation is not null)
        {
            string venvScriptPath = Path.Combine(virtualEnvironmentLocation, "Scripts");
            startInfo.FileName = Path.Combine(venvScriptPath, "pip.exe");
            startInfo.EnvironmentVariables["PATH"] = $"{venvScriptPath};{Environment.GetEnvironmentVariable("PATH")}";
        }

        using Process process = new() {  StartInfo = startInfo };
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException("Failed to install packages.");
        }
    }

    private void EnsureVirtualEnvironment(string path)
    {
        if (path is null)
        {
            throw new InvalidOperationException("Virtual environment location is not set.");
        }

        if (!Directory.Exists(path))
        {
            ProcessStartInfo startInfo = new()
            {
                WorkingDirectory = pythonLocation,
                FileName = "python",
                Arguments = $"-m venv {path}"
            };
            Process? process = Process.Start(startInfo);
            if (process is not null)
            {
                process.WaitForExit();
            }
            else
            {
                throw new InvalidOperationException("Failed to create virtual environment.");
            }
        }
    }

    public override bool Equals(object? obj) =>
        obj is PythonEnvironmentBuilder environment &&
        versionPath == environment.versionPath;

    public override int GetHashCode() => HashCode.Combine(versionPath, pythonLocation);

    internal void Destroy() => env = null;
}
