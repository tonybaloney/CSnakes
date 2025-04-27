using CSnakes.Runtime.CPython;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace CSnakes.Runtime.Python;

/****
 * Design notes
 * The Global Interpreter Lock (GIL) is a mutex in Python that makes Python thread-safe. 
 * The mutex is associated with a "thread state" object, which is an internal data structure in CPython.
 * The pointer to the thread state is stored in the current thread's TLS (Thread Local Storage).
 * 
 * Since .NET freely uses threads, we need to ensure that the GIL is acquired before calling any Python API.
 * Also, this class is designed to be reentrant, so that the GIL can be acquired multiple times by the same thread.
 * 
 * The "PyGilState" class is a disposable object that acquires the GIL when it is created and releases it when it is disposed.
 * 
 * IF a PyObject is allocated and then left for the GC to dispose, since the GC in .NET runs in a thread-pool, 
 * this can lead to some performance challenges where the GC taking and switching the GIL and the user code is also trying to acquire the GIL.
 * 
 * Some potential improvements - 
 *   - Consider saving the Python Thread State pointer in TLS and use the PyEval_SaveThread and PyEval_RestoreThread functions to save and restore the thread state.
 *   - Consider queuing the Dispose of PyObject/SafeHandles so they can be processed in collections, rather than switching the GIL in the finalizer thread. I don't know if the GC Finalizer spawns a thread per object or a thread pool?
 */

public static class GIL
{
    [ThreadStatic] private static PyGilState? currentState;
    [ThreadStatic] internal static nint pythonThreadState;
    private static ConcurrentQueue<nint> handlesToDispose = new();

    static GIL()
    {
        currentState = null;
        pythonThreadState = 0;
    }

    internal class PyGilState : IDisposable
    {
        private int recursionCount;

        public PyGilState()
        {
            if (pythonThreadState == 0)
            {
                pythonThreadState = CPythonAPI.PyThreadState_New();
            }
            Debug.Assert(CPythonAPI.IsInitialized);
            CPythonAPI.PyEval_RestoreThread(pythonThreadState);
            recursionCount = 1;
        }

        public void Ensure()
        {
            if (recursionCount == 0)
            {
                CPythonAPI.PyEval_RestoreThread(pythonThreadState);
            }
            recursionCount++;
        }

        public void Dispose()
        {
            if (recursionCount == 0)
            {
                Debug.WriteLine("GIL disposed more times than it was acquired by thread.");
                return;
            }
            recursionCount--;
            if (recursionCount > 0)
            {
                return;
            }
            // Before we release, take a few handles from the queue and dispose them
            GC.SuppressFinalize(this);
            while (handlesToDispose.TryDequeue(out nint handle))
            {
                CPythonAPI.Py_DecRefRaw(handle);
            }
            pythonThreadState = CPythonAPI.PyEval_SaveThread();
        }

        public int RecursionCount => recursionCount;
    }

    public readonly ref struct AcquiredLock : IDisposable
    {
        private readonly PyGilState state;

        internal AcquiredLock(PyGilState state) => this.state = state;

        public void Dispose() => state.Dispose();

        internal bool Equals(AcquiredLock other) => state == other.state;
    }

    public static AcquiredLock Acquire()
    {
        if (currentState == null)
        {
            currentState = new PyGilState();
        }
        else
        {
            currentState.Ensure();
        }
        return new(currentState);
    }

    internal static void QueueForDisposal(nint handle)
    {
        // Put the handle in a queue
        handlesToDispose.Enqueue(handle);
    }

    public static bool IsAcquired => currentState is { RecursionCount: > 0 };

    internal static void Require()
    {
        if (IsAcquired)
            return;
        throw new InvalidOperationException("This operation is invalid when the GIL is not acquired.");
    }
}
