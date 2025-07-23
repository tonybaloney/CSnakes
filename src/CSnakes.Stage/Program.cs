using CSnakes.Runtime;
using CSnakes.Runtime.Locators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.Reflection;

namespace CSnakes.Stage;

internal class Program
{
    static void Main(string[] args)
    {
        var versionString = Assembly.GetEntryAssembly()?
                                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                                    .InformationalVersion
                                    .ToString();

        Option<RedistributablePythonVersion> versionOption = new("--version", "The Python version to use (e.g., 3.12)")
        {
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
    }

    private static void Stage(RedistributablePythonVersion version = RedistributablePythonVersion.Python3_12)
    {
        Console.WriteLine($"Staging CSnakes for Python {version}...");

        var builder = Host.CreateApplicationBuilder();
        var home = Path.Join(Environment.CurrentDirectory);
        builder.Services
            .WithPython()
            .WithHome(home)
            .FromRedistributable(version);

        var app = builder.Build();

        var locator = app.Services.GetRequiredService<PythonLocator>();
        var location = locator.LocatePython();
        
        Console.WriteLine($"Python {version} downloaded and located at: {location.PythonPath}");    
    }
}
