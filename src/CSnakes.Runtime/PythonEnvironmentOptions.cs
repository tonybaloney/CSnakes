namespace CSnakes.Runtime;
public record PythonEnvironmentOptions(string Home, string[] ExtraPaths, bool InstallSignalHandlers = true, bool CaptureLogs = false);
