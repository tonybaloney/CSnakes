// See https://aka.ms/new-console-template for more information
using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

string pythonVersionWindows = "3.12.4";
string pythonVersionMacOS = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12";
string pythonVersionLinux = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12";


IHost app = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        var pb = services.WithPython();
        pb.WithHome(Path.Join(Environment.CurrentDirectory));

        pb.FromNuGet(pythonVersionWindows)
          .FromMacOSInstallerLocator(pythonVersionMacOS)
          .FromEnvironmentVariable("Python3_ROOT_DIR", pythonVersionLinux);
    })
    .Build();

IPythonEnvironment env = app.Services.GetRequiredService<IPythonEnvironment>();

var mod = env.App();
const int CYCLES = 100_000;
for (int i = 0; i < CYCLES; i++)
{
    var data = mod.GenerateData(1234, "hello", (3.2, "test"));
}