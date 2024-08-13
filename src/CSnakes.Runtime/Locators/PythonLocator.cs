namespace CSnakes.Runtime.Locators;

/// <summary>
/// Abstract class for locating Python installations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PythonLocator"/> class.
/// </remarks>
/// <param name="version">The version of Python.</param>
public abstract class PythonLocator(Version version)
{
    /// <summary>
    /// Gets the version of Python.
    /// </summary>
    protected Version Version { get ; } = version;

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
    /// Specifies whether the Python Locator is supported. Defaults to <see langword="true"/>
    /// </summary>
    /// <remarks>This would be used to do an OS check and ignore a particular <see cref="PythonLocator"/> if the OS does not match the one it supports. See the <see cref="WindowsInstallerLocator"/> as an example.</remarks>
    internal virtual bool IsSupported() => true;
}
