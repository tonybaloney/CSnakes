using CSnakes.Runtime.Locators;
using System.Runtime.InteropServices;


namespace CSnakes.Runtime.Tests.Locators;

public class RedistributableLocatorTests
{
    public readonly record struct DownloadUrlCase(
        OSPlatform Platform,
        Architecture Architecture,
        bool FreeThreaded,
        bool Debug,
        RedistributablePythonVersion Version
    )
    {
        public override string ToString()
            => $"{Platform}-{Architecture}-FT:{FreeThreaded}-DBG:{Debug}-{Version}";
    }

    public static IEnumerable<object[]> AllTestCases
    {
        get
        {
            foreach (var c in WindowsX64Cases())
                yield return new object[] { c };

            foreach (var c in WindowsArm64Cases())
                yield return new object[] { c };

            foreach (var c in MacLinuxCases())
                yield return new object[] { c };
        }
    }

    [Theory]
    [MemberData(nameof(AllTestCases))]
    public void ValidateDownloadUrl(DownloadUrlCase c)
        => VerifyDownloadUrl(c.Platform, c.Architecture, c.FreeThreaded, c.Debug, c.Version);

    private static IEnumerable<DownloadUrlCase> WindowsX64Cases()
    {
        yield return new(OSPlatform.Windows, Architecture.X64, false, false, RedistributablePythonVersion.Python3_10);
        yield return new(OSPlatform.Windows, Architecture.X64, false, false, RedistributablePythonVersion.Python3_11);
        yield return new(OSPlatform.Windows, Architecture.X64, false, false, RedistributablePythonVersion.Python3_12);
        yield return new(OSPlatform.Windows, Architecture.X64, false, false, RedistributablePythonVersion.Python3_13);
        yield return new(OSPlatform.Windows, Architecture.X64, true, false, RedistributablePythonVersion.Python3_13);
        yield return new(OSPlatform.Windows, Architecture.X64, false, false, RedistributablePythonVersion.Python3_14);
        yield return new(OSPlatform.Windows, Architecture.X64, true, false, RedistributablePythonVersion.Python3_14);
    }

    private static IEnumerable<DownloadUrlCase> WindowsArm64Cases()
    {
        yield return new(OSPlatform.Windows, Architecture.Arm64, false, false, RedistributablePythonVersion.Python3_11);
        yield return new(OSPlatform.Windows, Architecture.Arm64, false, false, RedistributablePythonVersion.Python3_12);
        yield return new(OSPlatform.Windows, Architecture.Arm64, false, false, RedistributablePythonVersion.Python3_13);
        yield return new(OSPlatform.Windows, Architecture.Arm64, false, false, RedistributablePythonVersion.Python3_14);
        yield return new(OSPlatform.Windows, Architecture.Arm64, true, false, RedistributablePythonVersion.Python3_13);
        yield return new(OSPlatform.Windows, Architecture.Arm64, true, false, RedistributablePythonVersion.Python3_14);
    }

    private static IEnumerable<DownloadUrlCase> MacLinuxCases()
    {
        var targets = new (OSPlatform Platform, Architecture Arch)[]
        {
            (OSPlatform.OSX,   Architecture.Arm64),
            (OSPlatform.OSX,   Architecture.X64),
            (OSPlatform.Linux, Architecture.Arm64),
            (OSPlatform.Linux, Architecture.X64),
        };

        // Non-debug, FreeThreaded = false for 3.9-3.14
        var nonDebugStdVersions = new[]
        {
            RedistributablePythonVersion.Python3_9,
            RedistributablePythonVersion.Python3_10,
            RedistributablePythonVersion.Python3_11,
            RedistributablePythonVersion.Python3_12,
            RedistributablePythonVersion.Python3_13,
            RedistributablePythonVersion.Python3_14,
        };
        foreach (var v in nonDebugStdVersions)
        {
            foreach (var (Platform, Arch) in targets)
                yield return new(Platform, Arch, false, false, v);
        }

        // Non-debug, FreeThreaded = true for 3.13-3.14
        var nonDebugFTVersions = new[]
        {
            RedistributablePythonVersion.Python3_13,
            RedistributablePythonVersion.Python3_14,
        };
        foreach (var v in nonDebugFTVersions)
        {
            foreach (var (Platform, Arch) in targets)
                yield return new(Platform, Arch, true, false, v);
        }

        // Debug, FreeThreaded = false for 3.11-3.14
        var debugStdVersions = new[]
        {
            RedistributablePythonVersion.Python3_11,
            RedistributablePythonVersion.Python3_12,
            RedistributablePythonVersion.Python3_13,
            RedistributablePythonVersion.Python3_14,
        };
        foreach (var v in debugStdVersions)
        {
            foreach (var (Platform, Arch) in targets)
                yield return new(Platform, Arch, false, true, v);
        }

        // Debug, FreeThreaded = true for 3.13-3.14
        var debugFTVersions = new[]
        {
            RedistributablePythonVersion.Python3_13,
            RedistributablePythonVersion.Python3_14,
        };
        foreach (var v in debugFTVersions)
        {
            foreach (var (Platform, Arch) in targets)
                yield return new(Platform, Arch, true, true, v);
        }
    }

    private static void VerifyDownloadUrl(OSPlatform platform, Architecture architecture, bool freeThreaded, bool debug, RedistributablePythonVersion version)
    {
        // Get URL from
        string url = RedistributableLocator.GetDownloadUrl(platform, architecture, freeThreaded, debug, version);

        using var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Head, url);
        var response = client.Send(request, TestContext.Current.CancellationToken);

        Assert.True(response.IsSuccessStatusCode, $"Failed to validate URL: {url}");
    }
}
