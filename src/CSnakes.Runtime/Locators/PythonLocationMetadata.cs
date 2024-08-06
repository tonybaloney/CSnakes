namespace CSnakes.Runtime.Locators;
/// <summary>
/// Metadata about the location of a Python installation.
/// </summary>
/// <param name="Folder">Path on disk where Python is to be loaded from.</param>
/// <param name="Version">Version of Python being used from the location.</param>
public sealed record PythonLocationMetadata(string Folder, string Version);
