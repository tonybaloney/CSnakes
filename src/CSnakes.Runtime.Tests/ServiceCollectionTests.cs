using CSnakes.Runtime.Locators;
using CSnakes.Runtime.PackageManagement;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace CSnakes.Runtime.Tests;

public class ServiceCollectionTests
{
    [Theory]
    [InlineData(" 3")]
    [InlineData(".3.0")]
    [InlineData("a.3.0")]
    public void ParsePythonVersion_throws_if_version_invalid(string givenVersion)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => _ = ServiceCollectionExtensions.ParsePythonVersion(givenVersion));
        Assert.Equal($"Invalid Python version: '{givenVersion}'", ex.Message);
    }

    [Theory]
    [InlineData("3")]
    [InlineData("3.0.0.0.0")]
    [InlineData("3.121213122322330")]
    public void ParsePythonVersion_throws_if_version_unparseable(string givenVersion)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => _ = ServiceCollectionExtensions.ParsePythonVersion(givenVersion));
        Assert.Equal($"Failed to parse Python version: '{givenVersion}'", ex.Message);
    }

    [Theory]
    [InlineData("3.12", 3, 12, 0, 0)]
    [InlineData("3.12.4", 3, 12, 4, 0)]
    [InlineData("3.13.0.beta.1", 3, 13, 0, 0)]
    [InlineData("3.13.0-rc1", 3, 13, 0, 0)]
    [InlineData("3.13.0.4-rev.5", 3, 13, 0, 4)]
    public void ParsePythonVersion_returns_expected(string givenVersion, int expectedMajor, int expectedMinor, int expectedBuild, int expectedPatch)
    {
        Version parsedVersion = ServiceCollectionExtensions.ParsePythonVersion(givenVersion);

        Assert.NotNull(parsedVersion);
        Assert.Equal(new Version(expectedMajor, expectedMinor, expectedBuild, expectedPatch), parsedVersion);
    }

    [Fact]
    public void FromNuGet_ShouldAddNuGetLocator()
    {
        IPythonEnvironmentBuilder builder = Substitute.For<IPythonEnvironmentBuilder>();
        ServiceCollection services = new();
        builder.Services.Returns(services);

        builder.FromNuGet("3.9.1");

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var locator = serviceProvider.GetService<PythonLocator>();
        Assert.NotNull(locator);
        Assert.IsType<NuGetLocator>(locator);
    }

    [Fact]
    public void FromWindowsStore_ShouldAddWindowsStoreLocator()
    {
        IPythonEnvironmentBuilder builder = Substitute.For<IPythonEnvironmentBuilder>();
        ServiceCollection services = new();
        builder.Services.Returns(services);

        builder.FromWindowsStore("3.9.1");

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var locator = serviceProvider.GetService<PythonLocator>();
        Assert.NotNull(locator);
        Assert.IsType<WindowsStoreLocator>(locator);
    }

    [Fact]
    public void FromWindowsInstaller_ShouldAddWindowsInstallerLocator()
    {
        IPythonEnvironmentBuilder builder = Substitute.For<IPythonEnvironmentBuilder>();
        ServiceCollection services = new();
        builder.Services.Returns(services);

        builder.FromWindowsInstaller("3.9.1");

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var locator = serviceProvider.GetService<PythonLocator>();
        Assert.NotNull(locator);
        Assert.IsType<WindowsInstallerLocator>(locator);
    }

    [Fact]
    public void FromMacOSInstallerLocator_ShouldAddMacOSInstallerLocator()
    {
        IPythonEnvironmentBuilder builder = Substitute.For<IPythonEnvironmentBuilder>();
        ServiceCollection services = new();
        builder.Services.Returns(services);

        builder.FromMacOSInstallerLocator("3.9.1");

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var locator = serviceProvider.GetService<PythonLocator>();
        Assert.NotNull(locator);
        Assert.IsType<MacOSInstallerLocator>(locator);
    }

    [Fact]
    public void FromSource_ShouldAddSourceLocator()
    {
        IPythonEnvironmentBuilder builder = Substitute.For<IPythonEnvironmentBuilder>();
        ServiceCollection services = new();
        builder.Services.Returns(services);

        builder.FromSource(folder: "/path/to/source", version: "3.9.1", debug: false, freeThreaded: false);

        var serviceProvider = services.BuildServiceProvider();
        var locator = serviceProvider.GetService<PythonLocator>();
        Assert.NotNull(locator);
        Assert.IsType<SourceLocator>(locator);
    }

    [Fact]
    public void FromEnvironmentVariable_ShouldAddEnvironmentVariableLocator()
    {
        IPythonEnvironmentBuilder builder = Substitute.For<IPythonEnvironmentBuilder>();
        ServiceCollection services = new();
        builder.Services.Returns(services);

        builder.FromEnvironmentVariable(environmentVariable: "PYTHON_HOME", version: "3.9.1");

        var serviceProvider = services.BuildServiceProvider();
        var locator = serviceProvider.GetService<PythonLocator>();
        Assert.NotNull(locator);
        Assert.IsType<EnvironmentVariableLocator>(locator);
    }

    [Fact]
    public void FromFolder_ShouldAddFolderLocator()
    {
        IPythonEnvironmentBuilder builder = Substitute.For<IPythonEnvironmentBuilder>();
        ServiceCollection services = new();
        builder.Services.Returns(services);

        builder.FromFolder(folder: "/path/to/source", version: "3.9.1");

        var serviceProvider = services.BuildServiceProvider();
        var locator = serviceProvider.GetService<PythonLocator>();
        Assert.NotNull(locator);
        Assert.IsType<FolderLocator>(locator);
    }

    [Fact]
    public void WithPipInstaller_ShouldAddPipInstaller()
    {
        IPythonEnvironmentBuilder builder = Substitute.For<IPythonEnvironmentBuilder>();
        ServiceCollection services = new();
        services.AddLogging();
        builder.Services.Returns(services);

        builder.WithPipInstaller();

        var serviceProvider = services.BuildServiceProvider();
        var installer = serviceProvider.GetService<IPythonPackageInstaller>();
        Assert.NotNull(installer);
        Assert.IsType<PipInstaller>(installer);
    }
}
