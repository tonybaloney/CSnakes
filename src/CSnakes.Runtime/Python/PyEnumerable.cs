using CSnakes.Runtime.CPython;
using System.Collections;
using static CSnakes.Runtime.Python.InternalServices;

namespace CSnakes.Runtime.Python;

internal class PyEnumerable<TValue, TImporter> : IEnumerable<TValue>, IEnumerator<TValue>, IDisposable
    where TImporter : IPyObjectImporter<TValue>
{
    private readonly PyObject _pyIterator;
    private TValue current = default!;

    internal PyEnumerable(PyObject pyObject)
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
            nint result = CPythonAPI.PyIter_Next(_pyIterator);
            if (result == IntPtr.Zero && CPythonAPI.PyErr_Occurred())
            {
                throw PyObject.ThrowPythonExceptionAsClrException();
            }

            if (result == IntPtr.Zero)
            {
                return false;
            }

            using PyObject pyObject = PyObject.Create(result);
            current = TImporter.Import(pyObject);
            return true;
        }
    }

    public void Reset() => throw new NotSupportedException("Python iterators cannot be reset");

    IEnumerator IEnumerable.GetEnumerator() => this;
}

internal class PyEnumerable<TValue> : PyEnumerable<TValue, PyObjectImporters.Runtime<TValue>>
{
    internal PyEnumerable(PyObject pyObject) : base(pyObject) { }
}
