namespace CSnakes.Runtime.Locators;
internal class FolderLocator(string folder, string version) : PythonLocator(version)
{
    public override PythonLocationMetadata LocatePython() =>
        LocatePythonInternal(folder);
}