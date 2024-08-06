namespace CSnakes.Runtime.Locators;
internal class EnvironmentVariableLocator(string variable, string version) : PythonLocator(version)
{
    public override PythonLocationMetadata LocatePython()
    {
        var envValue = Environment.GetEnvironmentVariable(variable);
        if (string.IsNullOrEmpty(envValue))
        {
            throw new ArgumentNullException($"Environment variable {variable} not found.");
        }

        return LocatePythonInternal(envValue);
    }
}