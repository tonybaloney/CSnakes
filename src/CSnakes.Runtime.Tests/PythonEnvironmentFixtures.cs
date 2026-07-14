using CSnakes.Runtime.Locators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

[assembly: AssemblyFixture(typeof(CSnakes.Runtime.Tests.RedistributablePythonSkuFixture))]

namespace CSnakes.Runtime.Tests;

public sealed class PythonEnvironmentFixtureBuildContext(IPythonEnvironmentBuilder environment,
                                                         ILoggingBuilder logging)
{
    public IPythonEnvironmentBuilder Environment { get; } = environment;
    public ILoggingBuilder Logging { get; } = logging;
}

/// <seealso href="https://xunit.net/docs/shared-context.html#collection-fixture"/>
public abstract class PythonEnvironmentFixtureBase : IDisposable
{
    private readonly IHost app;

    protected PythonEnvironmentFixtureBase(string? home = null,
                                           Action<PythonEnvironmentFixtureBuildContext>? configure = null)
    {
        var builder = Host.CreateApplicationBuilder();

        var environmentBuilder =
            builder.Services
                   .AddLogging(loggingBuilder => XUnitLoggerExtensions.AddXUnit(loggingBuilder))
                   .WithPython()
                   .WithHome(home ?? Environment.CurrentDirectory);

        configure?.Invoke(new(environmentBuilder, builder.Logging));

        this.app = builder.Build();
        Env = app.Services.GetRequiredService<IPythonEnvironment>();
    }

    public virtual void Dispose()
    {
        Env.Dispose();
        app.Dispose();
        GC.Collect();
    }

    public IPythonEnvironment Env { get; }

    protected IServiceProvider Services => app.Services;
}

public sealed class RedistributablePythonSkuFixture
{
    public RedistributablePythonSkuFixture()
    {
        var version = ServiceCollectionExtensions.ParsePythonVersion(Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12.9");

        if (version.Major is not 3)
            throw new NotSupportedException($"Python version {version.Major}.x is not supported.");

        Version = version.Minor switch
        {
             9 => RedistributablePythonVersion.Python3_9,
            10 => RedistributablePythonVersion.Python3_10,
            11 => RedistributablePythonVersion.Python3_11,
            12 => RedistributablePythonVersion.Python3_12,
            13 => RedistributablePythonVersion.Python3_13,
            14 => RedistributablePythonVersion.Python3_14,
            var minor => throw new NotSupportedException($"Python version 3.{minor} is not supported."),
        };

        VersionMajor = version.Major;
        VersionMinor = version.Minor;

        var debug = Debug = Environment.GetEnvironmentVariable("PYTHON_DEBUG") == "1";
        var freeThreaded = FreeThreaded = Environment.GetEnvironmentVariable("PYTHON_FREETHREADED") == "1";

        var venv = FormattableString.Invariant($".venv-{version.Major}.{version.Minor}{(freeThreaded ? "t" : "")}{(debug ? "d" : "")}");
        VirtualEnvironmentPath = Path.Join(Environment.CurrentDirectory, "python", venv);
    }

    public RedistributablePythonVersion Version { get; }

    public int VersionMajor { get; }
    public int VersionMinor { get; }

    public bool Debug { get; }
    public bool FreeThreaded { get; }

    public string VirtualEnvironmentPath { get; }
}

/// <seealso href="https://xunit.net/docs/shared-context.html#collection-fixture"/>
public abstract class RedistributablePythonEnvironmentFixture(RedistributablePythonSkuFixture sku,
                                                              string? home = null,
                                                              Action<PythonEnvironmentFixtureBuildContext>? configure = null) :
    PythonEnvironmentFixtureBase(home, context =>
    {
        context.Environment.FromRedistributable(sku.Version, debug: sku.Debug, freeThreaded: sku.FreeThreaded);
        configure?.Invoke(context);
    });
