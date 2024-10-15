using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.CPython.Unmanaged;
using pyoPtr = nint;

internal unsafe partial class CAPI
{
    protected static pyoPtr _PyDictType = IntPtr.Zero;
    public static pyoPtr PtrToPyDictType => _PyDictType;


    internal static nint PackDict(Span<string> kwnames, Span<pyoPtr> kwvalues)
    {
        var dict = PyDict_New();
        for (int i = 0; i < kwnames.Length; i ++)
        {
            var keyObj = AsPyUnicodeObject(kwnames[i]);
            int result = PyDict_SetItem(dict, keyObj, kwvalues[i]);
            if (result == -1)
            {
                throw PythonObject.ThrowPythonExceptionAsClrException();
            }
            Py_DecRef(keyObj);
        }
        return dict;
    }
}
