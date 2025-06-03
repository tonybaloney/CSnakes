using CSnakes.Runtime.Locators;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ITestOutputHelper = Xunit.Abstractions.ITestOutputHelper;

namespace RedistributablePython.Tests;
public class RedistributablePythonTestBase : IDisposable
{
    private readonly IPythonEnvironment env;
    private readonly IHost app;
    private readonly ITestOutputHelper _testOutputHelper;

    public RedistributablePythonTestBase(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        Version pythonVersionToTest = ServiceCollectionExtensions.ParsePythonVersion(Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12.9");
        bool freeThreaded = Environment.GetEnvironmentVariable("PYTHON_FREETHREADED") == "1";
        bool debugPython = Environment.GetEnvironmentVariable("PYTHON_DEBUG") == "1";
        string venvPath = Path.Join(Environment.CurrentDirectory, "python", ".venv");
        // If .venv exists, delete it
        if (Directory.Exists(venvPath))
        {
            Directory.Delete(venvPath, true);
        }

        RedistributablePythonVersion redistributableVersion = pythonVersionToTest.Minor switch
        {
            9 => RedistributablePythonVersion.Python3_9,
            10 => RedistributablePythonVersion.Python3_10,
            11 => RedistributablePythonVersion.Python3_11,
            12 => RedistributablePythonVersion.Python3_12,
            13 => RedistributablePythonVersion.Python3_13,
            14 => RedistributablePythonVersion.Python3_14,
            _ => throw new NotSupportedException($"Python version {pythonVersionToTest} is not supported.")
        };
        var builder = Host.CreateApplicationBuilder();
        var pb = builder.Services.WithPython();
        pb.WithHome(Path.Join(Environment.CurrentDirectory, "python"));

        pb.FromRedistributable(version: redistributableVersion, debug: debugPython, freeThreaded: freeThreaded)
          .WithUvInstaller()
          .WithVirtualEnvironment(venvPath);

        builder.Services.AddSingleton<ILoggerProvider>(new XUnitLoggerProvider(_testOutputHelper, appendScope: true));

        builder.Logging.SetMinimumLevel(LogLevel.Debug);
        builder.Logging.AddFilter(_ => true);
        
        app = builder.Build();

        env = app.Services.GetRequiredService<IPythonEnvironment>();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        GC.Collect();
    }

    public IPythonEnvironment Env => env;
}
