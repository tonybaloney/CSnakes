namespace CSnakes.Runtime.Locators;

/// <summary>
/// Metadata about the location of a Python installation.
/// </summary>
/// <param name="Folder">Path on disk where Python is to be loaded from.</param>
/// <param name="Version">Version of Python being used from the location.</param>
/// <param name="LibPythonPath">Path to the pythonXY.dll / libpythonX.Y.so / libpythonX.Y.dylib.</param>
/// <param name="PythonPath">PYTHONPATH to set when initializing the interpreter.</param>
/// <param name="PythonBinaryPath">Path to the python.exe / python binary.</param>
/// <param name="Debug">True if the Python installation is a debug build.</param>
/// <param name="FreeThreaded">True if the Python installation is free-threaded (3.13+).</param>
public sealed record PythonLocationMetadata(
    string Folder, // Path on disk where Python is to be loaded from
    Version Version, // Python version, e.g., 3.11
    string LibPythonPath, // Path to the pythonXY.dll / libpythonX.Y.so / libpythonX.Y.dylib
    string PythonPath, // PYTHONPATH to set when initializing the interpreter
    string PythonBinaryPath, // Python to python.exe / python binary
    bool Debug = false,
    bool FreeThreaded = false
);
