using CSnakes.Runtime.Locators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CSnakes.Runtime;
using CSnakes.Runtime.Python;

namespace StdLib.Tests;

public class StdLibTests
{
    private readonly IPythonEnvironment env;
    public StdLibTests()
    {
        var builder = Host.CreateApplicationBuilder();
        var pb = builder.Services.WithPython().FromRedistributable();

        builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddXUnit());

        builder.Logging.SetMinimumLevel(LogLevel.Debug);
        builder.Logging.AddFilter(_ => true);

        var app = builder.Build();

        env = app.Services.GetRequiredService<IPythonEnvironment>();
    }

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
