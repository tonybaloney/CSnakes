namespace CSnakes.Runtime.Locators;

internal class CondaLocator(string folder, Version version) : PythonLocator(version)
{
    public override PythonLocationMetadata LocatePython() =>
        LocatePythonInternal(folder);

    public string CondaHome { get { return folder; } }
}
