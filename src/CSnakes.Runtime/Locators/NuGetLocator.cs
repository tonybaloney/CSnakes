using System.Runtime.InteropServices;

namespace CSnakes.Runtime.Locators;

internal class NuGetLocator(string nugetVersion, Version version) : PythonLocator
{
    protected override Version Version { get; } = version;

    public override PythonLocationMetadata LocatePython()
    {
        var globalNugetPackagesPath = (NuGetPackages: Environment.GetEnvironmentVariable("NUGET_PACKAGES"),
                                       UserProfile  : Environment.GetEnvironmentVariable("USERPROFILE")) switch
            {
                (NuGetPackages : { Length: > 0 } path, _) => path,
                (_, UserProfile: { Length: > 0 } path) => Path.Combine(path, ".nuget", "packages"),
                _ => throw new DirectoryNotFoundException("Neither NUGET_PACKAGES or USERPROFILE environments variable were found, which are needed to locate the NuGet package cache.")
            };
        // TODO : Load optional path from nuget settings. https://learn.microsoft.com/en-us/nuget/consume-packages/managing-the-global-packages-and-cache-folders
        string nugetPath = Path.Combine(globalNugetPackagesPath, "python", nugetVersion, "tools");
        return LocatePythonInternal(nugetPath);
    }

    internal override bool IsSupported() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}
