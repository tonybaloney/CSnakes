using CSnakes.Runtime;
using System.Runtime.InteropServices;

namespace Integration.Tests;
public class TestEnvironment : IDisposable
{
    private readonly IPythonEnvironment env = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? PythonEnvironmentBuilder.FromNuGet("3.12.4")
                .Build(Path.Join(Environment.CurrentDirectory, "python"))
            : PythonEnvironmentBuilder.FromEnvironmentVariable("Python3_ROOT_DIR", "3.12")
                .Build(Path.Join(Environment.CurrentDirectory, "python"));


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            env.Dispose();
        }
    }

    public IPythonEnvironment Env => env;
}
