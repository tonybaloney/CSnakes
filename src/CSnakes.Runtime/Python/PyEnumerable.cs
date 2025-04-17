using CSnakes.Runtime.CPython;
using System.Collections;

namespace CSnakes.Runtime.Python;

internal class PyEnumerable<TValue, TImporter>(PyObject pyIterable) : IEnumerable<TValue>, IDisposable
    where TImporter : IPyObjectImporter<TValue>
{
    public void Dispose() => pyIterable.Dispose();

    public IEnumerator<TValue> GetEnumerator() => Iterator(pyIterable.GetIter());

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static IEnumerator<TValue> Iterator(PyObject pyIterator)
    {
        using var _ = pyIterator;

        IDisposable? gil = GIL.Acquire();

        try
        {
            while (true)
            {
                gil ??= GIL.Acquire(); // re-acquire GIL if necessary

                nint result = CPythonAPI.PyIter_Next(pyIterator);
                if (result == IntPtr.Zero)
                {
                    if (CPythonAPI.PyErr_Occurred())
                        throw PyObject.ThrowPythonExceptionAsClrException();

                    yield break;
                }

                TValue import;
                using (var itemObject = PyObject.Create(result))
                    import = TImporter.BareImport(itemObject);

                gil.Dispose(); // release GIL before yielding control
                gil = null;

                yield return import;
            }
        }
        finally
        {
            gil?.Dispose();
        }
    }
}
