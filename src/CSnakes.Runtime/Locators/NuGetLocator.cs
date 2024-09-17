using System.Runtime.InteropServices;

namespace CSnakes.Runtime.Locators;
internal class NuGetLocator(Version version) : PythonLocator(version)
{
    public override PythonLocationMetadata LocatePython()
    {
        var nugetPackagesOverride = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
        var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
        if (string.IsNullOrEmpty(userProfile) && string.IsNullOrEmpty(nugetPackagesOverride))
        {
            throw new DirectoryNotFoundException("Neither NUGET_PACKAGES or USERPROFILE environments variable were found, which are needed to locate the NuGet package cache.");
        }
        string? globalNugetPackagesPath = string.IsNullOrEmpty(nugetPackagesOverride) ? Path.Combine(userProfile, ".nuget", "packages") : nugetPackagesOverride;
        // TODO : Load optional path from nuget settings. https://learn.microsoft.com/en-us/nuget/consume-packages/managing-the-global-packages-and-cache-folders
        string nugetPath = Path.Combine(globalNugetPackagesPath!, "python", $"{Version.Major}.{Version.Minor}.{Version.Build}", "tools");
        return LocatePythonInternal(nugetPath);
    }

    internal override bool IsSupported() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}
