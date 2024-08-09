using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CSnakes.Runtime.Tests.Converter;
public class ConverterTestBase : IDisposable
{
    protected readonly IPythonEnvironment env;
    protected readonly IHost app;

    public ConverterTestBase()
    {
        app = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var pb = services.WithPython();
                pb.WithHome(Environment.CurrentDirectory);

                pb.FromSource("C:\\Users\\anthonyshaw\\source\\repos\\cpython\\", "3.12"); //FromNuGet("3.12.4").FromMacOSInstallerLocator("3.12").FromEnvironmentVariable("Python3_ROOT_DIR", "3.12.4");
            })
            .Build();

        env = app.Services.GetRequiredService<IPythonEnvironment>();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
        GC.Collect();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            app.Dispose();
        }
    }
}
