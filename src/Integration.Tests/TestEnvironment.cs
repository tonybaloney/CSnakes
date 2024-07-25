using PythonEnvironments;
using System.Runtime.InteropServices;

namespace Integration.Tests;
public class TestEnvironment : IDisposable
{
    IPythonEnvironment env;

    public TestEnvironment()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            var builder = PythonEnvironment.FromNuget("3.12.4") ?? throw new Exception("Cannot find Python"); // This is a dependency of the test project
            env = builder.Build(Path.Join(Environment.CurrentDirectory, "python"));
        } else
        {
            // Use the Github actions path
            var builder = PythonEnvironment.FromEnvironmentVariable("Python3_ROOT_DIR", "3.12") ?? throw new Exception("Cannot find Python");
            env = builder.Build(Path.Join(Environment.CurrentDirectory, "python"));
        }
    }

    public void Dispose()
    {
        env.Dispose();
    }

    public IPythonEnvironment Env { get { return env; } }
}
