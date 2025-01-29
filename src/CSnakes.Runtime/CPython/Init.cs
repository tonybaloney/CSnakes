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
    private static readonly Lock initLock = new();
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
            // Override default dlopen flags on linux to allow global symbol resolution (required in extension modules)
            // See https://github.com/tonybaloney/CSnakes/issues/112#issuecomment-2290643468
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
            Debug.WriteLine($"Initializing Python on thread {GetNativeThreadId()}");
            Py_Initialize();

            // Setup type statics
            using (GIL.Acquire())
            {
                if (PyErr_Occurred())
                    throw new InvalidOperationException("Python initialization failed.");

                if (!IsInitialized)
                    throw new InvalidOperationException("Python initialization failed.");

                // update sys.path by adding missing paths of PythonPath
                AppendMissingPathsToSysPath(PythonPath.Split(Path.PathSeparator));

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

                    // Acquire the GIL only to dispose it immediately because `PyGILState_Release`
                    // is not available after `Py_Finalize` is called. This is done primarily to
                    // trigger the disposal of handles that have been queued before the Python
                    // runtime is finalized.

                    GIL.Acquire().Dispose();

                    PyGILState_Ensure();
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

# region AppendMissingPathsToSysPath
    public void AppendMissingPathsToSysPath(string[] paths)
    {
        var pyoPtrSysName = AsPyUnicodeObject("sys");
        var pyoPtrSysModule = PyImport_Import(pyoPtrSysName);
        Py_DecRefRaw(pyoPtrSysModule);
        if (pyoPtrSysModule == IntPtr.Zero) PyObject.ThrowPythonExceptionAsClrException();
        var pyoPtrPathAttr = AsPyUnicodeObject("path");
        var pyoPtrPathList = PyObject_GetAttrRaw(pyoPtrSysModule, pyoPtrPathAttr);
        Py_DecRefRaw(pyoPtrPathAttr);
        if (pyoPtrSysModule == IntPtr.Zero) PyObject.ThrowPythonExceptionAsClrException();
        foreach (var path in paths)
        {
            var pyoPtrPythonPath = AsPyUnicodeObject(path);
            bool found = false;
            for (int i = 0; i < PyList_SizeRaw(pyoPtrPathList); i++)
            {
                var pyoPtrSysPath = PyList_GetItemRaw(pyoPtrPathList, i);
                if (pyoPtrSysPath == IntPtr.Zero)
                    continue;
                found = PyObject_RichCompareBoolRaw(pyoPtrPythonPath, pyoPtrSysPath, RichComparisonType.Equal) == 1;
                if (found)
                    break;
            }
            if (found == false)
                PyList_AppendRaw(pyoPtrPathList, pyoPtrPythonPath);
            Py_DecRefRaw(pyoPtrPythonPath);
        }
        Py_DecRefRaw(pyoPtrPathList);
        Py_DecRefRaw(pyoPtrSysModule);
    }
    #endregion
}
