using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    private const string PythonLibraryName = "python";
    public string PythonPath { get; internal set; } = string.Empty;

    private static string? pythonLibraryPath = null;

    public CPythonAPI(string pythonLibraryPath)
    {
        CPythonAPI.pythonLibraryPath = pythonLibraryPath;
        NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
    }

    private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName == PythonLibraryName)
        {
            return NativeLibrary.Load(pythonLibraryPath!, assembly, null);
        }
        return NativeLibrary.Load(libraryName, assembly, searchPath);
    }

    internal void Initialize()
    {
        Py_SetPath(PythonPath);
        Py_Initialize();
    }

    internal static bool IsInitialized => Py_IsInitialized() == 1;

    internal static string? Version
    {
        get
        {
            return Marshal.PtrToStringAnsi(Py_GetVersion());
        }
    }

    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr Py_GetVersion();

    [LibraryImport(PythonLibraryName)]
    internal static partial void Py_Initialize();

    [LibraryImport(PythonLibraryName)]
    internal static partial void Py_Finalize();

    [LibraryImport(PythonLibraryName)]
    internal static partial int Py_IsInitialized();

    [LibraryImport(PythonLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void Py_SetPath([MarshalAs(UnmanagedType.LPWStr)] string path);
}
