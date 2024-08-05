namespace CSnakes.Runtime.Locators;
internal class WindowsStoreLocator(string version) : PythonLocator(version)
{
    public override PythonLocationMetadata LocatePython()
    {
        var versionPath = MapVersion(Version);
        var windowsStorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", "Python" + versionPath);

        return LocatePythonInternal(windowsStorePath);
    }
}
