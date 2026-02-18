using CSnakes.Runtime;
using CSnakes.Runtime.EnvironmentManagement;
using CSnakes.Runtime.Locators;
using CSnakes.Runtime.PackageManagement;
using CSnakes.Stage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

const int defaultTimeout = 500; // Default timeout in seconds

return await ProgramArguments.CreateParser()
                             .WithVersion(ThisAssembly.Info.InformationalVersion)
                             .Parse(args)
                             .Match(Main,
                                    result => Print(Console.Out, RenderTemplate(result.Help)),
                                    result => Print(Console.Out, result.Version),
                                    result => Print(Console.Error, RenderTemplate(result.Usage), exitCode: 1));

static async Task<int> Main(ProgramArguments args)
{
    switch (args)
    {
        case { OptQuestion: true }:
        {
            Console.Out.WriteLine(ProgramArguments.Help);
            return 0;
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
                                                     : defaultTimeout);

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

static Task<int> Print(TextWriter writer, string message, int exitCode = 0)
{
    writer.WriteLine(message);
    return Task.FromResult(exitCode);
}

static string RenderTemplate(string input) =>
    TokenRegex().Replace(input, m => m.Value.AsSpan()[2..^1] switch
    {
        [.."version"] => ThisAssembly.Info.InformationalVersion,
        [.."bin"] => ThisAssembly.Constants.ToolCommandName,
        _ => m.Value,
    });

partial class Program
{
    // Pattern for "${...}" where the content starts and ends with a lowercase letter and
    // may contain lowercase letters and hyphens in the middle.
    [GeneratedRegex(@"\${[a-z]([a-z-]*[a-z])?}")]
    private static partial Regex TokenRegex();
}
