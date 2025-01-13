using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace RedistributablePython.Tests;
public class RedistributablePythonTestBase : IDisposable
{
    private readonly IPythonEnvironment env;
    private readonly IHost app;
    private readonly ITestOutputHelper _testOutputHelper;

    public RedistributablePythonTestBase(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        string venvPath = Path.Join(Environment.CurrentDirectory, "python", ".venv");
        app = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var pb = services.WithPython();
                pb.WithHome(Path.Join(Environment.CurrentDirectory, "python"));

                pb.FromRedistributable()
                  .WithUvInstaller()
                  .WithVirtualEnvironment(venvPath);

                services.AddSingleton<ILoggerProvider>(new XUnitLoggerProvider(_testOutputHelper, appendScope: true));

            })
            .ConfigureLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddFilter(_ => true);
            })
            .Build();

        env = app.Services.GetRequiredService<IPythonEnvironment>();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        GC.Collect();
    }

    public IPythonEnvironment Env => env;
}
