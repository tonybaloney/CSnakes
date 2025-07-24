using CSnakes.Runtime;
using CSnakes.Runtime.EnvironmentManagement;
using CSnakes.Runtime.Locators;
using CSnakes.Runtime.PackageManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.Reflection;

namespace CSnakes.Stage;

internal class Program
{
    const int DefaultTimeout = 500; // Default timeout in seconds
    static int Main(string[] args)
    {
        var versionString = Assembly.GetEntryAssembly()?
                                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                                    .InformationalVersion
                                    .ToString();

        Option<string> versionOption = new("--python")
        {
            Description = "The Python version to use (e.g., 3.12)",
            Required = true
        };
        Option<int> timeout = new("--timeout")
        {
            Description = $"The timeout in seconds for downloading Python (default is {DefaultTimeout} seconds)",
            Required = false
        };
        Option<string> venvPath = new("--venv")
        {
            Description = "The path to the virtual environment to create if required.",
            Required = false
        };
        Option<string> pipRequirements = new("--pip-requirements")
        {
            Description = "Path to a pip requirements file to install packages in the virtual environment.",
            Required = false
        };

        var rootCommand = new RootCommand
        {
            versionOption,
            timeout,
            venvPath,
            pipRequirements
        };

        rootCommand.Description = $"CSnakes.Stage v{versionString} - A tool to manage Python environments and versions.";

        rootCommand.SetAction((version) =>
        {
            Stage(version.GetRequiredValue(versionOption), version.GetValue(timeout), version.GetValue(venvPath), version.GetValue(pipRequirements));
        });

        ParseResult parseResult = rootCommand.Parse(args);
        return parseResult.Invoke();
    }

    private static void Stage(string version, int? timeout, string? venvPath, string? pipRequirements)
    {
        bool withVenv = !string.IsNullOrEmpty(venvPath);
        bool withPipRequirements = !string.IsNullOrEmpty(pipRequirements);

        Console.WriteLine($"Staging CSnakes for Python {version}...");

        var builder = Host.CreateApplicationBuilder();
        var home = Path.Join(Environment.CurrentDirectory);
        IPythonEnvironmentBuilder pythonEnvironmentBuilder = builder.Services
            .WithPython()
            .WithHome(home)
            .FromRedistributable(version: version, timeout: timeout ?? DefaultTimeout);

        if (withVenv)
        {
            pythonEnvironmentBuilder.WithVirtualEnvironment(venvPath!, ensureEnvironment: true);
        }

        if (withPipRequirements)
        {
            pythonEnvironmentBuilder.WithPipInstaller(pipRequirements!);
        }

        var app = builder.Build();

        var locator = app.Services.GetRequiredService<PythonLocator>();
        var location = locator.LocatePython();
        
        Console.WriteLine($"Python {version} downloaded and located at: {location.PythonBinaryPath}");

        if (withVenv)
        {
            Console.WriteLine("Creating virtual environment...");
            var environmentManager = app.Services.GetRequiredService<IEnvironmentManagement>();
            environmentManager.EnsureEnvironment(location);
            Console.WriteLine($"Virtual environment created at: {venvPath}");
        }

        if (withPipRequirements)
        {
            Console.WriteLine("Installing pip requirements...");
            var pipInstaller = app.Services.GetRequiredService<IPythonPackageInstaller>();
            pipInstaller.InstallPackagesFromRequirements(Environment.CurrentDirectory);
            Console.WriteLine($"Pip requirements installed from: {pipRequirements}");
        }

        Console.WriteLine("CSnakes staging completed successfully.");
    }
}
