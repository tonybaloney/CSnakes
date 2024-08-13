namespace CSnakes.Runtime.Tests;
public class ServiceCollectionTests
{
    [InlineData("3.12.4")]
    [InlineData("3.12")]
    [InlineData("3.13")]
    [InlineData("3.13.0.rc1")]
    [InlineData("3.13.0-rc1")]
    [Theory]
    public void parseVersionTest(string version)
    {
        var v = IServiceCollectionExtensions.ParsePythonVersion(version);
    }
}
