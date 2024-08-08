using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    private static nint PyFloatType = IntPtr.Zero;

    /// <summary>
    /// Create a PyFloat from the C double
    /// </summary>
    /// <param name="v"></param>
    /// <returns>A new reference to the float object</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyFloat_FromDouble(double value);

    /// <summary>
    /// Convery a PyFloat to a C double
    /// </summary>
    /// <param name="p"></param>
    /// <returns>The double value</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial double PyFloat_AsDouble(nint obj);

    internal static bool IsPyFloat(nint p)
    {
        return ((PyObjectStruct*)p)->Type() == PyFloatType;
    }
}
