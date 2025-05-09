using CSnakes.Runtime.Python;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Integration.Tests;

public class EnvironmentTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void TestEnvironment_DisposeAndCreateOther()
    {
        var number = Env.TestReload().TestNumber();
        Env.Dispose();
        IPythonEnvironment? otherEnv = null;

        try
        {
            otherEnv = CreateOtherEnvironment();
            var otherNumber = otherEnv.Other().TestOtherNumber();

            Assert.Equal(52, number);
            Assert.Equal(80, otherNumber);
        }
        finally
        {
            otherEnv?.Dispose();
            fixture.CreateEnvironment();
        }
    }

    private IPythonEnvironment CreateOtherEnvironment()
    {
        string pythonVersionWindows = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12.9";
        string pythonVersionMacOS = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12";
        string pythonVersionLinux = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12";
        bool freeThreaded = Environment.GetEnvironmentVariable("PYTHON_FREETHREADED") == "true";
        string venvPath = Path.Join(Environment.CurrentDirectory, "python_other", ".venv_other");

        var app = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var pb = services.WithPython();
                pb.WithHome(Path.Join(Environment.CurrentDirectory, "python_other"));

                pb.FromNuGet(pythonVersionWindows)
                    .FromMacOSInstallerLocator(pythonVersionMacOS, freeThreaded)
                    .FromWindowsStore("3.12")
                    .FromEnvironmentVariable("Python3_ROOT_DIR", pythonVersionLinux)
                    .WithVirtualEnvironment(venvPath);

                services.AddLogging(builder => builder.AddXUnit());
            })
            .Build();

        var env = app.Services.GetRequiredService<IPythonEnvironment>();
        return env;
    }
}
