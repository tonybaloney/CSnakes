using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tests;

public class PythonFixture : IDisposable
{
    private readonly IHost _host;
    public IPythonEnvironment PythonEnvironment => _host.Services.GetRequiredService<IPythonEnvironment>();

    public PythonFixture()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddPython("3.13", "Test");
        _host = builder.Build();
    }

    public void Dispose()
    {
        _host.Dispose();
        GC.SuppressFinalize(this);
    }
}

[CollectionDefinition("Python")]
public class PythonCollectionFixture : ICollectionFixture<PythonFixture>
{
    // This class has no code, and is never created.
    // Its purpose is to be the place to apply [CollectionDefinition]
    // and all the ICollectionFixture<> interfaces.
}
