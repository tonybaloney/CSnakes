using CSnakes.Runtime.Python;
using CSnakes.Runtime.Python.Interns;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI : IDisposable
{
    private const string PythonLibraryName = "csnakes_python";
    public string PythonPath { get; internal set; } = string.Empty;

    private static string? pythonLibraryPath = null;
    private static readonly object initLock = new();
    private static Version PythonVersion = new("0.0.0");
    private bool disposedValue = false;

    public CPythonAPI(string pythonLibraryPath, Version version)
    {
        PythonVersion = version;
        CPythonAPI.pythonLibraryPath = pythonLibraryPath;
        try
        {
            NativeLibrary.SetDllImportResolver(typeof(CPythonAPI).Assembly, DllImportResolver);
        }
        catch (InvalidOperationException)
        {
            // TODO: Work out how to call setdllimport resolver only once to avoid raising exceptions. 
            // Already set. 
        }
    }

    [Flags]
    private enum RTLD : int
    {
        LOCAL = 0,
        LAZY = 1,
        NOW = 2,
        NOLOAD = 4,
        DEEPBIND = 8,
        GLOBAL = 0x00100
    }

    [LibraryImport("libdl.so.2", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint dlopen(string path, int flags);

    private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName == PythonLibraryName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)){
                return dlopen(pythonLibraryPath!, (int)(RTLD.LAZY | RTLD.GLOBAL));
            }

            return NativeLibrary.Load(pythonLibraryPath!, assembly, null);
        }
        return NativeLibrary.Load(libraryName, assembly, searchPath);
    }

    internal void Initialize()
    {
        if (IsInitialized)
            return;

        InitializeEmbeddedPython();
    }

    private void InitializeEmbeddedPython()
    {
        lock (initLock)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Py_SetPath_UCS2_UTF16(PythonPath);
            else
                Py_SetPath_UCS4_UTF32(PythonPath);
            Debug.WriteLine($"Initializing Python on thread {GetNativeThreadId()}");
            Py_Initialize();

            // Setup type statics
            using (GIL.Acquire())
            {
                if (PyErr_Occurred())
                    throw new InvalidOperationException("Python initialization failed.");

                if (!IsInitialized)
                    throw new InvalidOperationException("Python initialization failed.");

                PyUnicodeType = GetTypeRaw(AsPyUnicodeObject(string.Empty));
                Py_True = PyBool_FromLong(1);
                Py_False = PyBool_FromLong(0);
                PyBoolType = GetTypeRaw(Py_True);
                PyEmptyTuple = PyTuple_New(0);
                PyTupleType = GetTypeRaw(PyEmptyTuple);
                PyFloatType = GetTypeRaw(PyFloat_FromDouble(0.0));
                PyLongType = GetTypeRaw(PyLong_FromLongLong(0));
                PyListType = GetTypeRaw(PyList_New(0));
                PyDictType = GetTypeRaw(PyDict_New());
                PyBytesType = GetTypeRaw(PyBytes_FromByteSpan(new byte[] { }));
                ItemsStrIntern = AsPyUnicodeObject("items");
                PyNone = GetBuiltin("None");
            }
            PyEval_SaveThread();
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
                lock (initLock)
                {
                    if (!IsInitialized)
                        return;

                    Debug.WriteLine("Calling Py_Finalize()");
                    Py_Finalize();
                }
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
