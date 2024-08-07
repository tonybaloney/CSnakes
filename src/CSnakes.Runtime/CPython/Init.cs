using System.Reflection;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    private const string PythonLibraryName = "csnakes_python";
    public string PythonPath { get; internal set; } = string.Empty;

    private static string? pythonLibraryPath = null;

    public CPythonAPI(string pythonLibraryPath)
    {
        CPythonAPI.pythonLibraryPath = pythonLibraryPath;
        try
        {
            NativeLibrary.SetDllImportResolver(typeof(CPythonAPI).Assembly, DllImportResolver);
        } catch (InvalidOperationException)
        {
            // Already set. 
        }
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
        if (IsInitialized)
            return;
        Py_SetPath(PythonPath);
        Py_Initialize();

        if (!IsInitialized)
            throw new InvalidOperationException("Python initialization failed.");

        // Setup type statics
        PyUnicodeType = ((PyObjectStruct*)AsPyUnicodeObject(String.Empty))->Type();
        Py_True = PyBool_FromLong(1);
        Py_False = PyBool_FromLong(0);
        PyBoolType = ((PyObjectStruct*)Py_True)->Type();
        PyEmptyTuple = PyTuple_New(0);
        PyTupleType = ((PyObjectStruct*)PyEmptyTuple)->Type();
        PyFloatType = ((PyObjectStruct*)PyFloat_FromDouble(0.0))->Type();
    }

    internal static bool IsInitialized => Py_IsInitialized() == 1;

    [LibraryImport(PythonLibraryName, StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(NonFreeUtf8StringMarshaller))]
    internal static partial string? Py_GetVersion();

    [LibraryImport(PythonLibraryName)]
    internal static partial void Py_Initialize();

    [LibraryImport(PythonLibraryName)]
    internal static partial void Py_Finalize();

    [LibraryImport(PythonLibraryName)]
    internal static partial int Py_IsInitialized();

    [LibraryImport(PythonLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void Py_SetPath([MarshalAs(UnmanagedType.LPWStr)] string path);
}
