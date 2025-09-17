using CSnakes.Runtime;
using CSnakes.Runtime.Locators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

#pragma warning disable RS0016 // FIXME Add public types and members to the declared API
#pragma warning disable RS0026 // FIXME Do not add multiple public overloads with optional parameters

namespace CSnakes;

public static class Python
{
    public static readonly RedistributablePythonVersion DefaultVersion = RedistributablePythonVersion.Python3_12;

    public static IPythonEnvironment GetEnvironment(string? home = null, LogLevel logLevel = LogLevel.Warning) =>
        GetEnvironment(DefaultVersion, home, logLevel);

    public static IPythonEnvironment GetEnvironment(RedistributablePythonVersion version, string? home = null,
                                                    LogLevel logLevel = LogLevel.Warning) =>
        GetEnvironment(builder => builder.WithHome(home ?? AppContext.BaseDirectory)
                                         .FromRedistributable(version),
            logLevel is not LogLevel.None ? builder => builder.SetMinimumLevel(logLevel) : null);

    public static IPythonEnvironment GetEnvironment(Action<IPythonEnvironmentBuilder> configure,
                                                    Action<ILoggingBuilder>? configureLogging = null)
    {
        var (env, _) = GetEnvironment(configure, configureLogging, static _ => (object?)null);
        return env;
    }

    public static (IPythonEnvironment, T) GetEnvironment<T>(Action<IPythonEnvironmentBuilder> configure,
                                                            Action<ILoggingBuilder>? configureLogging,
                                                            Func<IServiceProvider, T> selector)
    {
        if (PythonEnvironment.GetPythonEnvironment() is { } initializedEnv)
            return (initializedEnv, (T)initializedEnv.UserObject);

        var services = new ServiceCollection();

        if (configureLogging is { } someConfigureLogging)
            _ = services.AddLogging(someConfigureLogging);

        var builder = services.WithPython();
        configure(builder);

        if (services.FirstOrDefault(sd => sd.ServiceType == typeof(PythonLocator)) is null)
            _ = builder.FromRedistributable();

        services.AddSingleton<UserObjectFactory>(sp => _ => selector(sp));

        var serviceProvider = services.BuildServiceProvider();

        if (serviceProvider.GetService<IConfigureOptions<LoggerFilterOptions>>() is { } configureOptions)
        {
            var options = new LoggerFilterOptions();
            configureOptions.Configure(options);
            if (options.MinLevel is not LogLevel.None)
            {
                if (serviceProvider.GetService<ILoggerProvider>() is null)
                    _ = services.AddLogging(static builder =>
                    {
                        if (!OperatingSystem.IsBrowser() && !OperatingSystem.IsWasi())
                            _ = builder.AddConsole();
                        _ = builder.AddDebug();
                    });

                serviceProvider = services.BuildServiceProvider();
            }
        }

        var env = (PythonEnvironment)serviceProvider.GetRequiredService<IPythonEnvironment>();
        return (env, (T)env.UserObject);
    }
}
