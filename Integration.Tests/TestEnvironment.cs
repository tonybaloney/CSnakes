using PythonEnvironments;
using System.Runtime.InteropServices;

namespace Integration.Tests;
public class TestEnvironment : IDisposable
{
    IPythonEnvironment env;

    public TestEnvironment()
    {
        var userProfile = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Environment.GetEnvironmentVariable("USERPROFILE") : Environment.GetEnvironmentVariable("HOME");
        Assert.NotNull(userProfile);
        var builder = new PythonEnvironment(
            userProfile + Path.Join(".nuget", "packages", "python", "3.12.4", "tools"),
            "3.12.4");
        env = builder.Build(Path.Join(Environment.CurrentDirectory, "python"));
    }

    public void Dispose()
    {
        env.Dispose();
    }

    public IPythonEnvironment Env { get { return env; } }
}
