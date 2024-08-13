namespace CSnakes.Runtime.Locators;
internal class FolderLocator(string folder, Version version) : PythonLocator(version)
{
    public override PythonLocationMetadata LocatePython() =>
        LocatePythonInternal(folder);
}