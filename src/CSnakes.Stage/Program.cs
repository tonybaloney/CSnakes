using CSnakes.Runtime;
using CSnakes.Runtime.EnvironmentManagement;
using CSnakes.Runtime.Locators;
using CSnakes.Runtime.PackageManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        Option<string> uvRequirements = new("--uv-requirements")
        {
            Description = "Path to a pyproject.toml or requirements.txt to install packages in the virtual environment with uv.",
            Required = false
        };

        Option<bool> verbose = new("--verbose")
        {
            Description = "Enable verbose output.",
            Required = false
        };

        var rootCommand = new RootCommand
        {
            versionOption,
            timeout,
            venvPath,
            pipRequirements,
            uvRequirements,
            verbose
        };

        rootCommand.Description = $"setup-python v{versionString} - A tool to install Python environments and versions.";

        rootCommand.SetAction((version) =>
        {
            var config = new StageConfig
            {
                Version = version.GetRequiredValue(versionOption),
                Timeout = version.GetValue(timeout),
                VenvPath = version.GetValue(venvPath),
                PipRequirements = version.GetValue(pipRequirements),
                UvRequirements = version.GetValue(uvRequirements),
                Verbose = version.GetValue(verbose)
            };
            Stage(config);
        });

        ParseResult parseResult = rootCommand.Parse(args);
        return parseResult.Invoke();
    }

    private class StageConfig
    {
        public required string Version { get; set; }
        public int? Timeout { get; set; }
        public string? VenvPath { get; set; }
        public string? PipRequirements { get; set; }
        public string? UvRequirements { get; set; }
        public bool Verbose { get; set; }
    }

    private static void Stage(StageConfig config)
    {
        bool withVenv = !string.IsNullOrEmpty(config.VenvPath);
        bool withPipRequirements = !string.IsNullOrEmpty(config.PipRequirements);
        bool withUvRequirements = !string.IsNullOrEmpty(config.UvRequirements);

        if (withPipRequirements && withUvRequirements)
        {
            Console.WriteLine("Error: Both --pip-requirements and --uv-requirements are specified. Use only one.");
            throw new ArgumentException("Cannot specify both --pip-requirements and --uv-requirements.");
        }

        Console.WriteLine($"Staging CSnakes for Python {config.Version}...");

        var home = Path.Join(Environment.CurrentDirectory);
        var pyConfig =
            PythonEnvironmentConfiguration.Default
                                          .SetHome(home)
                                          .FromRedistributable(version: config.Version,
                                                               timeout: config.Timeout ?? DefaultTimeout);

        // Enable verbose logging if needed
        if (config.Verbose)
        {
            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.SetMinimumLevel(LogLevel.Debug);
            });
            pyConfig = pyConfig.WithLogger(services.BuildServiceProvider().GetRequiredService<ILoggerFactory>());
        }

        if (withVenv)
        {
            pyConfig = pyConfig.SetVirtualEnvironment(config.VenvPath!, ensureExists: true);
        }

        if (withPipRequirements)
        {
            pyConfig = pyConfig.AddPipInstaller(config.PipRequirements!);
        }

        if (withUvRequirements)
        {
            pyConfig = pyConfig.AddUvInstaller(config.UvRequirements!);
        }

        var locator = pyConfig.Locators.Single();
        var location = locator.LocatePython();

        Console.WriteLine($"Python {config.Version} downloaded and located at: {location.PythonBinaryPath}");

        if (pyConfig.EnvironmentManager is { } environmentManager)
        {
            Console.WriteLine("Creating virtual environment...");
            environmentManager.EnsureEnvironment(location);
            Console.WriteLine($"Virtual environment created at: {config.VenvPath}");
        }

        if (pyConfig.PackageInstallers.SingleOrDefault() is { } packageInstaller)
        {
            Console.WriteLine("Installing requirements...");
            packageInstaller.InstallPackagesFromRequirements(Environment.CurrentDirectory).GetAwaiter().GetResult();
            Console.WriteLine($"Python requirements installed from: {config.PipRequirements ?? config.UvRequirements}");
        }

        Console.WriteLine("CSnakes staging completed successfully.");
    }
}
