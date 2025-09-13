using CSnakes.Runtime.Python;
using CSnakes.Tests;

namespace StdLib.Tests;

public class StdLibTests(ITestOutputHelper? testOutputHelper = null)
{
    private readonly IPythonEnvironment env =
        PythonEnvironmentConfiguration.Default
                                      .FromRedistributable()
                                      .WithXUnitLogging(testOutputHelper)
                                      .GetPythonEnvironment();

    [Fact]
    public void TestStatisticsMode()
    {
        using var mod = env.Statistics();
        Assert.NotNull(mod);
        using var mode = mod.Mode(PyObject.From(new[] { 1, 2, 2, 3, 4 }));
        Assert.NotNull(mode);
        Assert.Equal(2, mode.As<long>());
    }
}
