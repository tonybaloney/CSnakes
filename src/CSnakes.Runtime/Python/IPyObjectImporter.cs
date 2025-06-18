using System.Diagnostics.CodeAnalysis;

namespace CSnakes.Runtime.Python;

[Experimental("PRTEXP001")]
public interface IPyObjectImporter<out T>
{
    /// <remarks>
    /// It is the responsibility of the caller to ensure that the GIL is
    /// acquired via <see cref="GIL.Acquire"/> when this method is invoked.
    /// </remarks>
    internal static abstract T BareImport(PyObject obj);
}
