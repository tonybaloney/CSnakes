using CSnakes.Runtime.CPython;
using System.Collections;

namespace CSnakes.Runtime.Python;

internal class PyEnumerable<TValue, TImporter>(PyObject pyIterable) : IPyIterable<TValue>, IDisposable
    where TImporter : IPyObjectImporter<TValue>
{
    public void Dispose() => pyIterable.Dispose();

    public IEnumerator<TValue> GetEnumerator() => Iterator(pyIterable.GetIter());

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static IEnumerator<TValue> Iterator(PyObject pyIterator)
    {
        using var _ = pyIterator;

        while (true)
        {
            TValue import;

            using (GIL.Acquire())
            {
                nint result = CPythonAPI.PyIter_Next(pyIterator);
                if (result == IntPtr.Zero)
                {
                    if (CPythonAPI.PyErr_Occurred())
                        throw PyObject.ThrowPythonExceptionAsClrException();

                    yield break;
                }

                using (var itemObject = PyObject.Create(result))
                    import = TImporter.BareImport(itemObject);
            }

            yield return import;
        }
    }
}
