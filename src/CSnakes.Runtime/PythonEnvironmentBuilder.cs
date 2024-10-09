﻿using CSnakes.Runtime.EnvironmentManagement;
using CSnakes.Runtime.Locators;
using Microsoft.Extensions.DependencyInjection;

namespace CSnakes.Runtime;

internal partial class PythonEnvironmentBuilder(IServiceCollection services) : IPythonEnvironmentBuilder
{
    private readonly string[] extraPaths = [];
    private string home = Environment.CurrentDirectory;

    public IServiceCollection Services { get; } = services;

    public IPythonEnvironmentBuilder WithVirtualEnvironment(string path, bool ensureExists = true)
    {
        Services.AddSingleton<IEnvironmentManagement>(new VenvEnvironmentManagement(path, ensureExists));
        return this;
    }

    public IPythonEnvironmentBuilder WithCondaEnvironment(string name, string environmentSpecPath, bool ensureExists = true)
    {
        Services.AddSingleton<IEnvironmentManagement>(
            sp => {
                try
                {
                    var condaLocator = sp.GetRequiredService<CondaLocator>();
                    var condaEnvManager = new CondaEnvironmentManagement(name, ensureExists, condaLocator, environmentSpecPath);
                    return condaEnvManager;
                }
                catch (InvalidOperationException)
                {
                    throw new InvalidOperationException("Conda environments much be used with Conda Locator.");
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
