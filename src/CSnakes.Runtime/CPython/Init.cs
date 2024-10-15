namespace CSnakes.Runtime.CPython;

internal unsafe partial class CAPI : Unmanaged.CAPI
{
    public CAPI(string pythonLibraryPath, Version version) : base(pythonLibraryPath, version) {}

}
