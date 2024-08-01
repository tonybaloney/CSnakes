namespace CSnakes.Runtime;
public partial class PythonEnvironmentBuilder
{
    /// <summary>
    /// Creates a Python environment from a NuGet package.
    /// </summary>
    /// <param name="version">The version of the Python package.</param>
    /// <returns>The Python environment.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when the USERPROFILE environment variable is not found.</exception>
    public static PythonEnvironmentBuilder FromNuGet(string version)
    {
        var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
        if (string.IsNullOrEmpty(userProfile))
        {
            throw new DirectoryNotFoundException("USERPROFILE environment variable not found, which is needed to locate the NuGet package cache.");
        }

        string nugetPath = Path.Combine(userProfile, ".nuget", "packages", "python", version, "tools");
        return FromFolder(nugetPath, version);
    }

    /// <summary>
    /// Creates a Python environment from the Windows Store installation.
    /// </summary>
    /// <param name="version">The version of Python.</param>
    /// <returns>The Python environment.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when the LocalApplicationData environment variable is not found.</exception>
    public static PythonEnvironmentBuilder FromWindowsStore(string version)
    {
        var versionPath = MapVersion(version);
        var windowsStorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", "Python" + versionPath);

        return FromFolder(windowsStorePath, version);
    }

    /// <summary>
    /// Creates a Python environment from the official Python Windows installer.
    /// </summary>
    /// <param name="version">The version of Python.</param>
    /// <returns>The Python environment.</returns>
    public static PythonEnvironmentBuilder FromPythonWindowsInstaller(string version)
    {
        var officialInstallerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Python", MapVersion(version, "."));

        return FromFolder(officialInstallerPath, version);
    }

    /// <summary>
    /// Creates a Python environment from an environment variable.
    /// </summary>
    /// <param name="variable">The name of the environment variable.</param>
    /// <param name="version">The version of Python.</param>
    /// <returns>The Python environment.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the environment variable is not found.</exception>
    public static PythonEnvironmentBuilder FromEnvironmentVariable(string variable, string version)
    {
        var envValue = Environment.GetEnvironmentVariable(variable);
        if (string.IsNullOrEmpty(envValue))
        {
            throw new ArgumentNullException($"Environment variable {variable} not found.");
        }

        return FromFolder(envValue, version);
    }

    /// <summary>
    /// Creates a Python environment from the PATH environment variable.
    /// </summary>
    /// <param name="version">The version of Python.</param>
    /// <returns>The Python environment.</returns>
    /// <exception cref="NullReferenceException">Thrown when the PATH environment variable is not found.</exception>
    public static PythonEnvironmentBuilder FromPath(string version)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(path))
        {
            throw new NullReferenceException("PATH environment variable not found.");
        }
        var pythonPath = path.Split(';').FirstOrDefault(p => p.Contains("Python" + version));
        if (string.IsNullOrEmpty(pythonPath))
        {
            throw new DirectoryNotFoundException($"Python {version} not found in PATH.");
        }

        return FromFolder(pythonPath, version);
    }

    /// <summary>
    /// Creates a Python environment from a specific folder.
    /// </summary>
    /// <param name="folder">The folder path.</param>
    /// <param name="version">The version of Python.</param>
    /// <returns>The Python environment.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when the folder is not found.</exception>
    public static PythonEnvironmentBuilder FromFolder(string folder, string version)
    {
        if (!Directory.Exists(folder))
        {
            throw new DirectoryNotFoundException($"Python not found at {folder}.");
        }

        return new PythonEnvironmentBuilder(folder, version);
    }
}
