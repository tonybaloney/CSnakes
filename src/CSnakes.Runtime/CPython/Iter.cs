using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    /// <summary>
    /// Return the next value from the iterator o. 
    /// The object must be an iterator according to PyIter_Check() 
    /// (it is up to the caller to check this).
    /// If there are no remaining values, returns NULL with no exception set.
    /// If an error occurs while retrieving the item, returns NULL and passes along the exception.
    /// </summary>
    /// <param name="iter"></param>
    /// <returns>New refernce to the next item</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyIter_Next(PythonObject iter);
}
