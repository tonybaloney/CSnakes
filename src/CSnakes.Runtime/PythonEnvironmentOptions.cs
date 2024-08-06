namespace CSnakes.Runtime;
public record PythonEnvironmentOptions(string Home, string? VirtualEnvironmentPath, bool EnsureVirtualEnvironment, string[] ExtraPaths);
