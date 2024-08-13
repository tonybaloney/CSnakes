using System.Runtime.InteropServices;

namespace CSnakes.Runtime.Locators;
internal class NuGetLocator(Version version) : PythonLocator(version)
{
    public override PythonLocationMetadata LocatePython()
    {
        var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
        if (string.IsNullOrEmpty(userProfile))
        {
            throw new DirectoryNotFoundException("USERPROFILE environment variable not found, which is needed to locate the NuGet package cache.");
        }

        string nugetPath = Path.Combine(userProfile, ".nuget", "packages", "python", $"{Version.Major}.{Version.Minor}.{Version.Build}", "tools");
        return LocatePythonInternal(nugetPath);
    }

    internal override bool IsSupported() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}
