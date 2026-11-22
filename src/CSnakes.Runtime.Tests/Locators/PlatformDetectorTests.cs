using CSnakes.Runtime.Locators;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.Tests.Locators;

public class PlatformDetectorTests
{
    [Fact]
    public void GetCurrentPlatform_MatchesRuntimeInformation()
    {
        OSPlatform platform = PlatformDetector.GetCurrentPlatform();

        Assert.True(RuntimeInformation.IsOSPlatform(platform), $"'{platform}' is not the current platform ('{RuntimeInformation.OSDescription}').");
    }
}
