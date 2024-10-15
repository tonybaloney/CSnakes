using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.CPython;
using pyoPtr = nint;

internal unsafe partial class CAPI
{
    /// <summary>
    /// Return the object from dictionary p which has a key `key`. 
    /// </summary>
    /// <param name="dict">Dictionary Object</param>
    /// <param name="key">Key Object</param>
    /// <exception cref="KeyNotFoundException">If the key is not found</exception>
    /// <returns>New reference.</returns>
    internal static pyoPtr GetItemInPyDict(MPyOPtr dict, MPyOPtr key)
    {
        var result = PyDict_GetItem(dict.DangerousGetHandle(), key.DangerousGetHandle());
        if (result == IntPtr.Zero)
        {
            throw PythonObject.ThrowPythonExceptionAsClrException();
        }
        Py_IncRef(result);
        return result;
    }

    /// <summary>
    /// Does the dictionary contain the key? Raises exception on failure
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    internal static bool IsKeyInPyDict(MPyOPtr key, MPyOPtr dict)
    {
        int result = PyDict_Contains(dict.DangerousGetHandle(), key.DangerousGetHandle());
        if(result == -1)
        {
            throw PythonObject.ThrowPythonExceptionAsClrException();
        }
        return result == 1;
    }


}
