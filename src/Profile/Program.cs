// See https://aka.ms/new-console-template for more information
using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
ILogger logger = app.Services.GetRequiredService<ILogger<Program>>();

var mod = env.App();
const int CYCLES = 100_000;
var stopwatch = new System.Diagnostics.Stopwatch();
stopwatch.Start();
for (int i = 0; i < CYCLES; i++)
{
    var data = mod.GenerateData(i, "hello", (3.2, "testinput"), (i % 1 == 0));
}
stopwatch.Stop();
logger.LogInformation($"Took {stopwatch.ElapsedMilliseconds}ms for {CYCLES} cycles");

// Run the test again but with the result that returns PyObject (no conversion)
stopwatch.Restart();
for (int i = 0; i < CYCLES; i++)
{
    var data = mod.GenerateDataAny(i, "hello", (3.2, "testinput"), (i % 1 == 0));
}
stopwatch.Stop();
logger.LogInformation($"Took {stopwatch.ElapsedMilliseconds}ms for {CYCLES} cycles without conversions");

// Run the test again with a function that takes no arguments and returns nothing.
stopwatch.Restart();
for (int i = 0; i < CYCLES; i++)
{
    mod.EmptyFunction();
}
stopwatch.Stop();
logger.LogInformation($"Took {stopwatch.ElapsedMilliseconds}ms for {CYCLES} cycles with no input or output");

logger.LogInformation("Done"); 