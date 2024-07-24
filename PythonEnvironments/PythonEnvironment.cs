using Python.Runtime;
using System.Runtime.InteropServices;

namespace PythonEnvironments;

public class PythonEnvironment(string pythonLocation, string version = "3.10.0")
{
    private readonly string versionPath = MapVersion(version);
    private PythonEnvironmentInternal? env;
    private string[] extraPaths = [];

    private static string MapVersion(string version, string sep = "")
    {
        // split on . then take the first two segments and join them without spaces
        var versionParts = version.Split('.');
        return string.Join(sep, versionParts.Take(2));
    }

    public PythonEnvironment WithVirtualEnvironment(string path)
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

        env = new PythonEnvironmentInternal(pythonLocation, version, home, this, this.extraPaths);

        return env;
    }

    // Helper factories
    public static PythonEnvironment? FromNuget(string version)
    {
        var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
        if (string.IsNullOrEmpty(userProfile))
        {
            return null;
        }
        return new PythonEnvironment(
            userProfile + Path.Combine(".nuget", "packages", "python", version, "tools"),
            version);
    }

    public static PythonEnvironment? FromWindowsStore(string version)
    {
        var versionPath = MapVersion(version);
        var windowsStorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", "Python" + versionPath);
        if (!Directory.Exists(windowsStorePath))
        {
            return null;
        }
        return new PythonEnvironment(windowsStorePath, version);
    }

    public static PythonEnvironment? FromPythonWindowsInstaller(string version)
    {
        var officialInstallerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Python", MapVersion(version, "."));

        if (!Directory.Exists(officialInstallerPath))
        {
            return null;
        }
        return new PythonEnvironment(officialInstallerPath, version);
    }

    public static PythonEnvironment? FromEnvironmentVariable(string variable, string version)
    {
        var envValue = Environment.GetEnvironmentVariable(variable);
        if (string.IsNullOrEmpty(envValue))
        {
            return null;
        }

        if (!Directory.Exists(envValue))
        {
            return null;
        }

        return new PythonEnvironment(envValue, version);
    }

    public override bool Equals(object obj)
    {
        return obj is PythonEnvironment environment &&
               versionPath == environment.versionPath;
    }

    public override int GetHashCode()
    {
        int hashCode = 955711454;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(versionPath);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(pythonLocation);
        return hashCode;
    }

    internal class PythonEnvironmentInternal : IPythonEnvironment
    {
        private readonly PythonEnvironment pythonEnvironment;

        public PythonEnvironmentInternal(string pythonLocation, string version, string home, PythonEnvironment pythonEnvironment, string[] extraPath)
        {
            string versionPath = MapVersion(version);
            string majorVersion = MapVersion(version, ".");
            // Don't use BinaryFormatter by default as it's deprecated in .NET 8
            // See https://github.com/pythonnet/pythonnet/issues/2282
            RuntimeData.FormatterFactory = () =>
            {
                return new NoopFormatter();
            };
            string pythonLibraryPath = string.Empty;
            string sep = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ";" : ":";
           
            // Add standard library to PYTHONPATH
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Runtime.PythonDLL = Path.Combine(pythonLocation, $"python{versionPath}.dll");
                PythonEngine.PythonPath = Path.Combine(pythonLocation, "Lib") + sep + Path.Combine(pythonLocation, "DLLs");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Runtime.PythonDLL = Path.Combine(pythonLocation, "lib", $"libpython{majorVersion}.dylib");
                PythonEngine.PythonPath = Path.Combine(pythonLocation, "lib", $"python{majorVersion}") + sep + Path.Combine(pythonLocation, "lib", $"python{majorVersion}", "lib-dynload");
            }
            else 
            {
                Runtime.PythonDLL = Path.Combine(pythonLocation, "lib", $"libpython{majorVersion}.so");
                PythonEngine.PythonPath = Path.Combine(pythonLocation, "lib", $"python{majorVersion}") + sep + Path.Combine(pythonLocation, "lib", $"python{majorVersion}", "lib-dynload");
            }

            if (!string.IsNullOrEmpty(home))
            {
                PythonEngine.PythonPath = PythonEngine.PythonPath + sep + home;
            }

            if (extraPath.Length > 0)
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

        public void Dispose()
        {
            try
            {
                //PythonEngine.Shutdown();
            }
            catch (NotImplementedException) // Thrown from NoopFormatter
            {
                // Ignore
            }
            pythonEnvironment.Destroy();
        }
    }

    private void Destroy()
    {
        env = null;
    }
}

public interface IPythonEnvironment : IDisposable
{
}
