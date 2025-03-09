using CSnakes.Runtime.Python;
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
    private Task? initializationTask = null;
    private readonly ManualResetEventSlim disposalEvent = new();

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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
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

        /* Notes:
         * 
         * The CPython initialization and finalization
         * methods should be called from the same thread.
         * 
         * Without doing so, CPython can hang on finalization.
         * Especially if the code imports the threading module, or 
         * uses asyncio.
         * 
         * Since we have no guarantee that the Dispose() of this 
         * class will be called from the same managed CLR thread
         * as the one which called Init(), we create a Task with 
         * a cancellation token and use that as a mechanism to wait
         * in the background for shutdown then call the finalization.
         */

        var initializationTaskCompletionSource = new TaskCompletionSource();
        initializationTask = Task.Run(() =>
        {
            try
            {
                InitializeEmbeddedPython();
            }
            catch (Exception ex)
            {
                initializationTaskCompletionSource.SetException(ex);
                return;
            }
            initializationTaskCompletionSource.SetResult();
            disposalEvent.Wait();
            FinalizeEmbeddedPython();
        });

        initializationTaskCompletionSource.Task.GetAwaiter().GetResult();
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
                AsyncioModule = Import("asyncio");
                NewEventLoopFactory = AsyncioModule.GetAttr("new_event_loop");
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

    protected void FinalizeEmbeddedPython()
    {
        lock (initLock)
        {
            if (!IsInitialized)
                return;

            // Shut down asyncio coroutines
            CloseEventLoops();

            // Clean-up interns
            NewEventLoopFactory?.Dispose();
            AsyncioModule?.Dispose();
            // TODO: Add more cleanup code here

            Debug.WriteLine($"Calling Py_Finalize() on thread {GetNativeThreadId()}");

            // Acquire the GIL only to dispose it immediately because `PyGILState_Release`
            // is not available after `Py_Finalize` is called. This is done primarily to
            // trigger the disposal of handles that have been queued before the Python
            // runtime is finalized.

            GIL.Acquire().Dispose();

            PyGILState_Ensure();
            Py_Finalize();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (initializationTask != null)
                {
                    if (disposalEvent == null)
                        throw new InvalidOperationException("Invalid runtime state");

                    disposalEvent.Set();

                    try
                    {
                        initializationTask.GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error during finalization: {ex}");
                    }
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
