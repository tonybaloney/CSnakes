using CSnakes.Runtime;

#pragma warning disable IDE0130 // Namespace does not match folder structure
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for configuring Python environments in a dependency injection container.
/// </summary>
public static class PythonExtensions
{
    /// <summary>
    /// Adds a Python environment to the service collection.
    /// </summary>
    /// <param name="services">The service collection to which the Python environment will be added.</param>
    /// <param name="version">The version of Python to use.</param>
    /// <param name="environment">The name of the Python environment being configured.</param>
    /// <returns>An <see cref="IPythonEnvironmentBuilder"/> for further configuration of the Python environment.</returns>
    /// <remarks>
    /// This method configures a Python environment with a specified version and service name. It sets up
    /// a virtual environment, enables the use of pip for package installation, and uses a redistributable
    /// Python runtime.
    /// </remarks>
    public static IPythonEnvironmentBuilder AddPython(this IServiceCollection services, string version, string? environment = null)
    {
        var home = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "python", ".");
        return services
            .WithPython()
            .WithHome(home)
            .WithVirtualEnvironment(Path.Join(home, $"{environment ?? string.Empty}.venv")) // Supports conda environments!
            .WithPipInstaller()
            .FromRedistributable(version);
    }

    public static long TotalMicroseconds(this TimeSpan value) =>
        value.Ticks / TimeSpan.TicksPerMicrosecond;

    public static long TotalMicroseconds(this TimeOnly value) =>
        value.Ticks / TimeSpan.TicksPerMicrosecond;
}
