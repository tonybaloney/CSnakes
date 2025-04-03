using System.Runtime.InteropServices;

namespace CSnakes.Runtime.Locators;
internal class WindowsInstallerLocator(Version version) : PythonLocator
{
    protected override Version Version { get; } = version;

    readonly string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

    public override PythonLocationMetadata LocatePython()
    {
        var officialInstallerPath = Path.Combine(programFilesPath, "Python", $"{Version.Major}.{Version.Minor}");

        return LocatePythonInternal(officialInstallerPath);
    }

    internal override bool IsSupported() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}
