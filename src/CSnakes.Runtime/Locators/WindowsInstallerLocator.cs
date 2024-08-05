namespace CSnakes.Runtime.Locators;
internal class WindowsInstallerLocator(string version) : PythonLocator(version)
{
    readonly string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

    public override PythonLocationMetadata LocatePython()
    {
        string mappedVersion = MapVersion(Version, ".");
        var officialInstallerPath = Path.Combine(programFilesPath, "Python", mappedVersion);

        return LocatePythonInternal(officialInstallerPath);
    }
}
