using CSnakes.Runtime;
using CSnakes.Runtime.Locators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.Reflection;

namespace CSnakes.Stage;

internal class Program
{
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
        var rootCommand = new RootCommand
        {
            versionOption
        };

        rootCommand.Description = $"CSnakes.Stage v{versionString} - A tool to manage Python environments and versions.";

        rootCommand.SetAction((version) =>
        {
            Stage(version.GetRequiredValue(versionOption));
        });

        ParseResult parseResult = rootCommand.Parse(args);
        return parseResult.Invoke();
    }

    private static void Stage(string version)
    {
        Console.WriteLine($"Staging CSnakes for Python {version}...");

        var builder = Host.CreateApplicationBuilder();
        var home = Path.Join(Environment.CurrentDirectory);
        builder.Services
            .WithPython()
            .WithHome(home)
            .FromRedistributable(version, timeout: 500);

        var app = builder.Build();

        var locator = app.Services.GetRequiredService<PythonLocator>();
        var location = locator.LocatePython();
        
        Console.WriteLine($"Python {version} downloaded and located at: {location.PythonPath}");    
    }
}
