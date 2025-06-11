using Microsoft.Extensions.Hosting;

namespace CSnakes.Runtime.Tests;
public class PythonEnvironmentBuilderTests
{
    [Fact]
    public void Environment_SetHome_ShouldSetHomeDirectory()
    {
        var builder = Host.CreateApplicationBuilder();
        var services = builder.Services;
        string expectedHome = "/path/to/python/home";
        var pb = new PythonEnvironmentBuilder(services);
        pb.WithHome(expectedHome);
        var opts = pb.GetOptions();
        Assert.Equal(expectedHome, opts.Home);
    }

    [Fact]
    public void Environment_DisableSignalHandlers_ShouldSetFlag()
    {
        var builder = Host.CreateApplicationBuilder();
        var services = builder.Services;
        var pb = new PythonEnvironmentBuilder(services);
        Assert.True(pb.GetOptions().InstallSignalHandlers);
        pb.DisableSignalHandlers();
        Assert.False(pb.GetOptions().InstallSignalHandlers);
    }
}
