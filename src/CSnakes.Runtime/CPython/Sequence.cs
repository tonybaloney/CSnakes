using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    public static bool IsPySequence(PythonObject p)
    {
        return PySequence_Check(p) == 1;
    }

    /// <summary>
    /// Return 1 if the object provides the sequence protocol, and 0 otherwise.
    /// Note that it returns 1 for Python classes with a __getitem__() method, 
    /// unless they are dict subclasses, since in general it is impossible to determine 
    /// what type of keys the class supports. This function always succeeds.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial int PySequence_Check(PythonObject ob);

    /// <summary>
    /// Return the number of items in the sequence object as ssize_t
    /// </summary>
    /// <param name="seq">Sequence object</param>
    /// <returns>Number of items in the sequence</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PySequence_Size(PythonObject seq);

    /// <summary>
    /// Return the ith element of o, or NULL on failure. This is the equivalent of the Python expression o[i].
    /// </summary>
    /// <param name="seq">Sequence object</param>
    /// <param name="index">Index</param>
    /// <returns>New reference to the item or NULL.</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PySequence_GetItem(PythonObject seq, nint index);
}
