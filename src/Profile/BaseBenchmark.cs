using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Profile;
public class BaseBenchmark
{
    protected readonly IPythonEnvironment Env;

    public BaseBenchmark()
    {
        string pythonVersionWindows = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12.9";
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

        Env = app.Services.GetRequiredService<IPythonEnvironment>();
    }
}
