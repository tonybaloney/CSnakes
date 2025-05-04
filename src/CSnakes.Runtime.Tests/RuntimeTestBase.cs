using CSnakes.Runtime.Locators;
using Microsoft.Extensions.Logging;

namespace CSnakes.Runtime.Tests;

public class RuntimeTestBase : IDisposable
{
    protected readonly IPythonEnvironment env;

    public RuntimeTestBase()
    {
        Version pythonVersionToTest = ServiceCollectionExtensions.ParsePythonVersion(Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12.9");
        bool freeThreaded = Environment.GetEnvironmentVariable("PYTHON_FREETHREADED") == "1";
        bool debugPython = Environment.GetEnvironmentVariable("PYTHON_DEBUG") == "1";

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

        env = CSnakes.Python.GetEnvironment(
                  pb => pb.WithHome(Environment.CurrentDirectory)
                          .FromRedistributable(version: redistributableVersion, debug: debugPython, freeThreaded: freeThreaded),
                  static lb => lb.AddXUnit());
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        GC.Collect();
    }
}
