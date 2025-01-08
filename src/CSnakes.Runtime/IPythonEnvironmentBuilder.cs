using Microsoft.Extensions.DependencyInjection;

namespace CSnakes.Runtime;

/// <summary>
/// Represents a builder for creating Python environments.
/// </summary>
public interface IPythonEnvironmentBuilder
{
    public IServiceCollection Services { get; }

    /// <summary>
    /// Sets the virtual environment path for the Python environment being built.
    /// </summary>
    /// <param name="path">The path to the virtual environment.</param>
    /// <param name="ensureEnvironment">Indicates whether to ensure the virtual environment exists.</param>
    /// <returns>The current instance of the <see cref="IPythonEnvironmentBuilder"/>.</returns>
    IPythonEnvironmentBuilder WithVirtualEnvironment(string path, bool ensureEnvironment = true);


    /// <summary>
    /// Sets the virtual environment path for the Python environment to a named conda environment.
    /// This requires Python to be installed via Conda and usage of the <see cref="ServiceCollectionExtensions.FromConda(IPythonEnvironmentBuilder, string)"/> locator. 
    /// </summary>
    /// <param name="name">The name of the conda environment to use.</param>
    /// <param name="environmentSpecPath">The path to the conda environment specification file (environment.yml), used if ensureEnvironment = true.</param>
    /// <param name="ensureEnvironment">Indicates whether to create the conda environment if it doesn't exist (not yet supported).</param>
    /// <returns>The current instance of the <see cref="IPythonEnvironmentBuilder"/>.</returns>
    IPythonEnvironmentBuilder WithCondaEnvironment(string name, string? environmentSpecPath = null, bool ensureEnvironment = false, string? pythonVersion = null);

    /// <summary>
    /// Sets the home directory for the Python environment being built.
    /// </summary>
    /// <param name="home">The home directory path.</param>
    /// <returns>The current instance of the <see cref="IPythonEnvironmentBuilder"/>.</returns>
    IPythonEnvironmentBuilder WithHome(string home);

    /// <summary>
    /// Gets the options for the Python environment being built.
    /// </summary>
    /// <returns>The <see cref="PythonEnvironmentOptions"/> for the current environment.</returns>
    PythonEnvironmentOptions GetOptions();
}
