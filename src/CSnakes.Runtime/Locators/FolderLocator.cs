namespace CSnakes.Runtime.Locators;
internal class FolderLocator(string folder, Version version) : PythonLocator
{
    protected override Version Version { get; } = version;

    public override PythonLocationMetadata LocatePython() =>
        LocatePythonInternal(folder);
}
