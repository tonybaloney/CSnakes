using CSnakes.Runtime;

namespace CSnakes.Tests;

public class VersionParsingTests
{
    [Fact]
    public void TestParsingVersions() {
        Assert.Equal("3.12.3.0", ServiceCollectionExtensions.ParsePythonVersion("3.12.3").ToString());
        Assert.Equal("3.10.3.0", ServiceCollectionExtensions.ParsePythonVersion("3.10.3").ToString());
        Assert.Equal("3.14.0.0", ServiceCollectionExtensions.ParsePythonVersion("3.14.0a5").ToString());
        Assert.Equal("3.14.0.0", ServiceCollectionExtensions.ParsePythonVersion("3.14.0-alpha.5").ToString());
    }
}
