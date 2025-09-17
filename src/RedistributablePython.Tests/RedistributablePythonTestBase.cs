using CSnakes.Runtime.Locators;
using CSnakes.Tests;

namespace RedistributablePython.Tests;
public class RedistributablePythonTestBase : IDisposable
{
    private readonly IPythonEnvironment env;

    public RedistributablePythonTestBase(ITestOutputHelper? testOutputHelper = null)
    {
        Version pythonVersionToTest = ServiceCollectionExtensions.ParsePythonVersion(Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12.9");
        bool freeThreaded = Environment.GetEnvironmentVariable("PYTHON_FREETHREADED") == "1";
        bool debugPython = Environment.GetEnvironmentVariable("PYTHON_DEBUG") == "1";
        string venvPath = Path.Join(Environment.CurrentDirectory, "python", $".venv-{pythonVersionToTest}{(freeThreaded ? "t" : "")}{(debugPython ? "d" : "")}");

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

        env = PythonEnvironmentConfiguration.Default
                                            .SetHome(Path.Join(Environment.CurrentDirectory, "python"))
                                            .DisableSignalHandlers()
                                            .FromRedistributable(version: redistributableVersion, debug: debugPython, freeThreaded: freeThreaded)
                                            .SetVirtualEnvironment(venvPath)
                                            .AddUvInstaller()
                                            .WithXUnitLogging(testOutputHelper)
                                            .GetPythonEnvironment();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        GC.Collect();
    }

    public IPythonEnvironment Env => env;
}
