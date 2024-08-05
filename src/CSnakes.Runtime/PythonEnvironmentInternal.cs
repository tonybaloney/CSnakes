using CSnakes.Runtime.Locators;
using Python.Runtime;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime;

internal class PythonEnvironmentInternal : IPythonEnvironment
{
    private readonly PythonEnvironmentBuilder pythonEnvironment;

    public PythonEnvironmentInternal(PythonLocationMetadata pythonLocation, string home, PythonEnvironmentBuilder pythonEnvironment, string[] extraPath)
    {
        string versionPath = PythonEnvironmentBuilder.MapVersion(pythonLocation.Version);
        string majorVersion = PythonEnvironmentBuilder.MapVersion(pythonLocation.Version, ".");
        // Don't use BinaryFormatter by default as it's deprecated in .NET 8
        // See https://github.com/pythonnet/pythonnet/issues/2282
        RuntimeData.FormatterFactory = () =>
        {
            return new NoopFormatter();
        };
        string pythonLibraryPath = string.Empty;

        char sep = Path.PathSeparator;

        SetupStandardLibrary(pythonLocation.Folder, versionPath, majorVersion, sep);

        if (!string.IsNullOrEmpty(home))
        {
            PythonEngine.PythonPath = PythonEngine.PythonPath + sep + home;
        }

        if (extraPath is { Length: > 0 })
        {
            PythonEngine.PythonPath = PythonEngine.PythonPath + sep + string.Join(sep, extraPath);
        }

        try
        {
            PythonEngine.Initialize();
        }
        catch (NullReferenceException)
        {

        }

        this.pythonEnvironment = pythonEnvironment;
    }

    private static void SetupStandardLibrary(string pythonLocation, string versionPath, string majorVersion, char sep)
    {
        // Add standard library to PYTHONPATH
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Python.Runtime.Runtime.PythonDLL = Path.Combine(pythonLocation, $"python{versionPath}.dll");
            PythonEngine.PythonPath = Path.Combine(pythonLocation, "Lib") + sep + Path.Combine(pythonLocation, "DLLs");
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Python.Runtime.Runtime.PythonDLL = Path.Combine(pythonLocation, "lib", $"libpython{majorVersion}.dylib");
            PythonEngine.PythonPath = Path.Combine(pythonLocation, "lib", $"python{majorVersion}") + sep + Path.Combine(pythonLocation, "lib", $"python{majorVersion}", "lib-dynload");
            return;
        }

        Python.Runtime.Runtime.PythonDLL = Path.Combine(pythonLocation, "lib", $"libpython{majorVersion}.so");
        PythonEngine.PythonPath = Path.Combine(pythonLocation, "lib", $"python{majorVersion}") + sep + Path.Combine(pythonLocation, "lib", $"python{majorVersion}", "lib-dynload");
    }

    public void Dispose() => pythonEnvironment.Destroy();
}
