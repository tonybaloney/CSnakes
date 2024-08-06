namespace CSnakes.Runtime.Locators;

/// <summary>
/// Abstract class for locating Python installations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PythonLocator"/> class.
/// </remarks>
/// <param name="version">The version of Python.</param>
public abstract class PythonLocator(string version)
{
    /// <summary>
    /// Gets the version of Python.
    /// </summary>
    protected string Version { get; } = version;

    /// <summary>
    /// Locates the Python installation.
    /// </summary>
    /// <returns>The metadata of the located Python installation.</returns>
    public abstract PythonLocationMetadata LocatePython();

    /// <summary>
    /// Locates the Python installation internally.
    /// </summary>
    /// <param name="folder">The folder path to search for Python.</param>
    /// <returns>The metadata of the located Python installation.</returns>
    /// <exception cref="DirectoryNotFoundException">Python not found at the specified folder.</exception>
    protected PythonLocationMetadata LocatePythonInternal(string folder)
    {
        if (!Directory.Exists(folder))
        {
            throw new DirectoryNotFoundException($"Python not found at {folder}.");
        }

        return new PythonLocationMetadata(folder, Version);
    }

    /// <summary>
    /// Maps the Python version to a specific format.
    /// </summary>
    /// <param name="version">The version of Python.</param>
    /// <param name="sep">The separator to use in the mapped version.</param>
    /// <returns>The mapped version of Python.</returns>
    protected static string MapVersion(string version, string sep = "")
    {
        // split on . then take the first two segments and join them without spaces
        var versionParts = version.Split('.');
        return string.Join(sep, versionParts.Take(2));
    }

    internal virtual bool IsSupported() => true;
}
