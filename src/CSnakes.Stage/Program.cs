using CSnakes.Runtime;
using CSnakes.Runtime.EnvironmentManagement;
using CSnakes.Runtime.Locators;
using CSnakes.Runtime.PackageManagement;
using CSnakes.Stage;
using DocoptNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Runtime.CompilerServices;

return await ProgramArguments.CreateParser()
                             .WithVersion(BuildConstants.InformationalVersion)
                             .Parse(args)
                             .Match(Main,
                                    result => Print(result.Help),
                                    result => Print(result.Version),
                                    result => Print(result.Usage, exitCode: 1));

static async Task<int> Main(ProgramArguments args)
{
    switch (args)
    {
        case { OptQuestion: true }:
        {
            return await Print(ProgramArguments.Help);
        }
        case { OptPython: { } pythonVersion }:
        {
            Console.WriteLine($"Staging CSnakes for Python {pythonVersion}...");

            var builder = Host.CreateApplicationBuilder();
            var home = Path.Join(Environment.CurrentDirectory);
            var pythonEnvironmentBuilder =
                builder.Services
                       .WithPython()
                       .WithHome(home)
                       .FromRedistributable(version: pythonVersion,
                                            timeout: args.OptTimeout is { } timeout
                                                     ? int.Parse(timeout, NumberStyles.None, CultureInfo.InvariantCulture)
                                                     : BuildConstants.DefaultTimeoutInSeconds);

            if (args.OptVerbose) // Enable verbose logging if needed
            {
                pythonEnvironmentBuilder.Services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConsole();
                    loggingBuilder.SetMinimumLevel(LogLevel.Debug);
                });
            }

            if (args.OptVenv is { } venvPath)
            {
                pythonEnvironmentBuilder.WithVirtualEnvironment(venvPath, ensureEnvironment: true);
            }

            string? requirementsFilePath = null;

            if (args.OptPipRequirements is { Length: > 0 } pipRequirementsFilePath)
            {
                pythonEnvironmentBuilder.WithPipInstaller(pipRequirementsFilePath);
                requirementsFilePath = pipRequirementsFilePath;
            }
            else if (args.OptUvRequirements is { Length: > 0 } uvRequirementsFilePath)
            {
                pythonEnvironmentBuilder.WithUvInstaller(uvRequirementsFilePath);
                requirementsFilePath = uvRequirementsFilePath;
            }

            var app = builder.Build();

            var locator = app.Services.GetRequiredService<PythonLocator>();
            var location = locator.LocatePython();

            Console.WriteLine($"Python {pythonVersion} downloaded and located at: {location.PythonBinaryPath}");

            if (app.Services.GetService<IEnvironmentManagement>() is { } environmentManager)
            {
                Console.WriteLine("Creating virtual environment...");
                environmentManager.EnsureEnvironment(location);
                Console.WriteLine($"Virtual environment created at: {args.OptVenv}");
            }

            if (requirementsFilePath is { } someRequirementsFilePath
                && app.Services.GetService<IPythonPackageInstaller>() is { } packageInstaller)
            {
                Console.WriteLine("Installing requirements...");
                await packageInstaller.InstallPackagesFromRequirements(Environment.CurrentDirectory);
                Console.WriteLine($"Python requirements installed from: {someRequirementsFilePath}");
            }

            Console.WriteLine("CSnakes staging completed successfully.");
            return 0;
        }
        default:
        {
            throw new SwitchExpressionException();
        }
    }
}

static Task<int> Print(string message, TextWriter? writer = null, int exitCode = 0)
{
    writer ??= exitCode is 0 ? Console.Out : Console.Error;
    writer.WriteLine(message.ReplaceLineEndings().AsSpan().TrimEnd(['\r', '\n']));
    return Task.FromResult(exitCode);
}

[DocoptArguments]
partial class ProgramArguments
{
    const string BinName = BuildConstants.ToolCommandName;

    public const string Help = $"""
        Tool to install Python environments and versions.

        Usage:
          {BinName}
              [--verbose]
              [--timeout=SECONDS]
              [--venv=PATH]
              [--pip-requirements=FILE | --uv-requirements=FILE]
              --python=VERSION
          {BinName} -h | --help | -?
          {BinName} --version

        Options:
          -?, -h, --help           Show help and usage information
          --version                Show version information
          --verbose                Enable verbose output
          --python=VERSION         Python version to use (e.g., 3.12)
          --timeout=SECONDS        Timeout in seconds for downloading Python
                                     (default is {BuildConstants.DefaultTimeoutInSecondsString} seconds)
          --venv=PATH              Path to the virtual environment to create if
                                     required
          --pip-requirements=FILE  Path to a pip requirements file to install packages
                                     in the virtual environment.
          --uv-requirements=FILE   Path to a "pyproject.toml" or "requirements.txt" to
                                     install packages in the virtual environment with
                                     uv.


        It automatically downloads the appropriate Python redistributable for your
        platform, optionally creates virtual environments, and can install Python
        packages from requirements files.

        This tool is designed for pre-creating Python environments in Docker images for
        use in CSnakes projects, but can be used as a general-purpose tool for
        installing Python.

        For more information, visit:
        https://tonybaloney.github.io/CSnakes/v1/user-guide/deployment/

        """;
}
