using CSnakes.Runtime.Python;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI : IDisposable
{
    private const string PythonLibraryName = "csnakes_python";
    public string PythonPath { get; internal set; } = string.Empty;

    private static string? pythonLibraryPath = null;
    private static readonly object initLock = new();
    private bool disposedValue;

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
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Py_SetPath_UCS2_UTF16(PythonPath);
        else
            Py_SetPath_UCS4_UTF32(PythonPath);
        lock (initLock)
        {
            Py_Initialize();

            // Setup type statics
            using (GIL.Acquire())
            {
                if (PyErr_Occurred() == 1)
                    throw new InvalidOperationException("Python initialization failed.");

                if (!IsInitialized)
                    throw new InvalidOperationException("Python initialization failed.");

                PyUnicodeType = ((PyObjectStruct*)AsPyUnicodeObject(String.Empty))->Type();
                Py_True = PyBool_FromLong(1);
                Py_False = PyBool_FromLong(0);
                PyBoolType = ((PyObjectStruct*)Py_True)->Type();
                PyEmptyTuple = PyTuple_New(0);
                PyTupleType = ((PyObjectStruct*)PyEmptyTuple)->Type();
                PyFloatType = ((PyObjectStruct*)PyFloat_FromDouble(0.0))->Type();
                PyLongType = ((PyObjectStruct*)PyLong_FromLongLong(0))->Type();
                PyListType = ((PyObjectStruct*)PyList_New(0))->Type();
                PyDictType = ((PyObjectStruct*)PyDict_New())->Type();

                // Import builtins module
                var builtinsMod = Import("builtins");
                PyNone = GetAttr(builtinsMod, "None");
                Py_DecRef(builtinsMod);
            }
        }
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

    [LibraryImport(PythonLibraryName, EntryPoint = "Py_SetPath")]
    internal static partial void Py_SetPath_UCS2_UTF16([MarshalAs(UnmanagedType.LPWStr)] string path);

    [LibraryImport(PythonLibraryName, EntryPoint = "Py_SetPath", StringMarshallingCustomType = typeof(Utf32StringMarshaller), StringMarshalling = StringMarshalling.Custom)]
    internal static partial void Py_SetPath_UCS4_UTF32(string path);

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (!IsInitialized)
                    return;
                Py_Finalize();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
