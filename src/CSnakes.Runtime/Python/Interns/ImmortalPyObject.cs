namespace CSnakes.Runtime.Python.Interns;

internal class ImmortalPyObject : PythonObject
{
    internal ImmortalPyObject(nint handle) : base(handle)
    {
    }

    protected override bool ReleaseHandle() => true;


    protected override void Dispose(bool disposing)
    {
        // I am immortal!!
    }
}
