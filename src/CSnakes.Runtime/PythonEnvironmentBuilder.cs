using Python.Runtime;

namespace CSnakes.Runtime;

public partial class PythonEnvironmentBuilder
{
    private readonly string versionPath;
    private readonly string pythonLocation;
    private readonly string version;
    private PythonEnvironmentInternal? env;
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

    public PythonEnvironmentBuilder WithVirtualEnvironment(string path)
    {
        extraPaths = [.. extraPaths, path, Path.Combine(path, "Lib", "site-packages")];
        return this;
    }

    public IPythonEnvironment Build(string home)
    {
        if (PythonEngine.IsInitialized && env is not null)
        {
            // Raise exception?
            return env;
        }

        env = new PythonEnvironmentInternal(pythonLocation, version, home, this, extraPaths);

        return env;
    }

    public override bool Equals(object? obj) =>
        obj is PythonEnvironmentBuilder environment &&
        versionPath == environment.versionPath;

    public override int GetHashCode() => HashCode.Combine(versionPath, pythonLocation);

    internal void Destroy() => env = null;
}
