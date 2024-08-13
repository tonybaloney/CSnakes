namespace CSnakes.Runtime.Locators;
internal class PathLocator(Version version) : PythonLocator(version)
{
    public override PythonLocationMetadata LocatePython()
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(path))
        {
            throw new NullReferenceException("PATH environment variable not found.");
        }
        var pythonPath = path.Split(';').FirstOrDefault(p => p.Contains("Python" + Version));
        if (string.IsNullOrEmpty(pythonPath))
        {
            throw new DirectoryNotFoundException($"Python {Version} not found in PATH.");
        }

        return LocatePythonInternal(pythonPath);
    }
}