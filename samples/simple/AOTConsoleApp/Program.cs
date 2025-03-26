using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        var home = Path.Join(Environment.CurrentDirectory, "..", "..", "..", "..", "ExamplePythonDependency");
        var venv = Path.Join(home, ".venv");
        services
        .WithPython()
        .WithHome(home)
        .WithVirtualEnvironment(venv)
        .FromNuGet("3.12.4")
        .FromMacOSInstallerLocator("3.12")
        .FromEnvironmentVariable("Python3_ROOT_DIR", "3.12")
        .WithPipInstaller();
    });

var app = builder.Build();

var env = app.Services.GetRequiredService<IPythonEnvironment>();

RunQuickDemo(env);

static void RunQuickDemo(IPythonEnvironment env)
{
    var quickDemo = env.QuickDemo();
    Console.WriteLine(quickDemo.Scream("a", 99));
    Console.WriteLine(string.Join(',', quickDemo.ScreamNames(["a", "b", "c"], 3)));
}
