using CSnakes.Runtime.EnvironmentManagement;
using CSnakes.Runtime.Locators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSnakes.Runtime;

internal partial class PythonEnvironmentBuilder(IServiceCollection services) : IPythonEnvironmentBuilder
{
    private readonly string[] extraPaths = [];
    private string home = Environment.CurrentDirectory;

    public IServiceCollection Services { get; } = services;

    public IPythonEnvironmentBuilder WithVirtualEnvironment(string path, bool ensureExists = true)
    {
        Services.AddSingleton<IEnvironmentManagement>(
            sp =>
            {
                var logger = sp.GetService<ILogger<VenvEnvironmentManagement>>();
                return new VenvEnvironmentManagement(logger, path, ensureExists);
            });
        return this;
    }

    public IPythonEnvironmentBuilder WithCondaEnvironment(string name, string? environmentSpecPath = null, bool ensureEnvironment = false, string? pythonVersion = null)
    {
        Services.AddSingleton<IEnvironmentManagement>(
            sp =>
            {
                try
                {
                    var condaLocator = sp.GetRequiredService<CondaLocator>();
                    var logger = sp.GetService<ILogger<CondaEnvironmentManagement>>();
                    var condaEnvManager = new CondaEnvironmentManagement(logger, name, ensureEnvironment, condaLocator, environmentSpecPath, pythonVersion);
                    return condaEnvManager;
                }
                catch (InvalidOperationException)
                {
                    throw new InvalidOperationException("Conda environments must be used with Conda Locator.");
                }
            });
        return this;
    }

    public IPythonEnvironmentBuilder WithHome(string home)
    {
        this.home = home;
        return this;
    }

    public PythonEnvironmentOptions GetOptions() =>
        new(home, extraPaths);
}
