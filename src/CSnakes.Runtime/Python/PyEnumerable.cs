using CSnakes.Runtime.CPython;
using System.Collections;

namespace CSnakes.Runtime.Python;

internal class PyEnumerable<TValue, TImporter> : IEnumerable<TValue>, IEnumerator<TValue>, IDisposable
    where TImporter : IPyObjectImporter<TValue>
{
    private readonly PythonObject _pyIterator;
    private TValue current = default!;

    internal PyEnumerable(PythonObject pyObject)
    {
        using (GIL.Acquire())
        {
            _pyIterator = pyObject.GetIter();
        }
    }

    public TValue Current => current;

    object IEnumerator.Current => current!;

    public void Dispose() => _pyIterator.Dispose();

    public IEnumerator<TValue> GetEnumerator() => this;

    public bool MoveNext()
    {
        using (GIL.Acquire())
        {
            nint result = CAPI.PyIter_Next(_pyIterator);
            if (result == IntPtr.Zero && CAPI.IsPyErrOccurred())
            {
                throw PythonObject.ThrowPythonExceptionAsClrException();
            }

            if (result == IntPtr.Zero)
            {
                return false;
            }

            using PythonObject pyObject = PythonObject.Create(result);
            current = TImporter.Import(pyObject);
            return true;
        }
    }

    public void Reset() => throw new NotSupportedException("Python iterators cannot be reset");

    IEnumerator IEnumerable.GetEnumerator() => this;
}

internal class PyEnumerable<TValue> : PyEnumerable<TValue, PyObjectImporter<TValue>>
{
    internal PyEnumerable(PythonObject pyObject) : base(pyObject) { }
}
