using Microsoft.Extensions.DependencyInjection;

namespace CSnakes.Runtime;

internal partial class PythonEnvironmentBuilder(IServiceCollection services) : IPythonEnvironmentBuilder
{
    private bool ensureVirtualEnvironment = false;
    private string? virtualEnvironmentLocation;
    private string[] extraPaths = [];
    private string home = Environment.CurrentDirectory;

    public IServiceCollection Services { get; } = services;

    public IPythonEnvironmentBuilder WithVirtualEnvironment(string path, bool ensureVirtualEnvironment = true)
    {
        this.ensureVirtualEnvironment = ensureVirtualEnvironment;
        virtualEnvironmentLocation = path;
        extraPaths = [.. extraPaths, path, Path.Combine(virtualEnvironmentLocation, "Lib", "site-packages")];

        return this;
    }

    public IPythonEnvironmentBuilder WithHome(string home)
    {
        this.home = home;
        return this;
    }

    public PythonEnvironmentOptions GetOptions() =>
        new(home, virtualEnvironmentLocation, ensureVirtualEnvironment, extraPaths);
}
