using System.Reflection;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    private const string PythonLibraryName = "python";

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
            return NativeLibrary.Load(pythonLibraryPath!, assembly, DllImportSearchPath.UserDirectories);
        }
        // This shouldn't happen.
        return IntPtr.Zero;
    }

    [LibraryImport(PythonLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial string Py_GetVersion();
}
