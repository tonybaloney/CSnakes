using System;
using CSnakes.Runtime.Locators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

public sealed record RedistributablePythonSku
{
    public RedistributablePythonSku(int versionMajor, int versionMinor)
    {
        if (versionMajor is not 3)
            throw new ArgumentException($"Python version {versionMajor}.x is not supported.", nameof(versionMajor));

        Version = versionMinor switch
        {
             9 => RedistributablePythonVersion.Python3_9,
            10 => RedistributablePythonVersion.Python3_10,
            11 => RedistributablePythonVersion.Python3_11,
            12 => RedistributablePythonVersion.Python3_12,
            13 => RedistributablePythonVersion.Python3_13,
            14 => RedistributablePythonVersion.Python3_14,
            var minor => throw new ArgumentException($"Python version 3.{minor} is not supported.", nameof(versionMinor)),
        };

        VersionMajor = versionMajor;
        VersionMinor = versionMinor;
    }

    public RedistributablePythonVersion Version { get; }

    public int VersionMajor { get; }
    public int VersionMinor { get; }

    public bool Debug { get; init; }
    public bool FreeThreaded { get; init; }
}

/// <seealso href="https://xunit.net/docs/shared-context.html#collection-fixture"/>
public abstract class RedistributablePythonEnvironmentFixture(
                          string? home = null,
                          Action<PythonEnvironmentFixtureBuildContext, RedistributablePythonSku>? configure = null) :
    PythonEnvironmentFixtureBase(home, context =>
    {
        var version = ServiceCollectionExtensions.ParsePythonVersion(Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12.9");
        var sku = new RedistributablePythonSku(version.Major, version.Minor)
        {
            Debug = Environment.GetEnvironmentVariable("PYTHON_DEBUG") == "1",
            FreeThreaded = Environment.GetEnvironmentVariable("PYTHON_FREETHREADED") == "1",
        };

        context.Environment.FromRedistributable(sku.Version, debug: sku.Debug, freeThreaded: sku.FreeThreaded);

        configure?.Invoke(context, sku);
    });
