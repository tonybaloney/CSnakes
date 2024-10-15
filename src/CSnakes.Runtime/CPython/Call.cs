namespace CSnakes.Runtime.CPython;
using pyoPtr = nint;

internal unsafe partial class CAPI
{
    internal static nint Call(MPyOPtr callable, Span<pyoPtr> args) => Call(callable.DangerousGetHandle(), args);

    internal static IntPtr Call(MPyOPtr callable, Span<pyoPtr> args, Span<string> kwnames, Span<pyoPtr> kwvalues)
        => Call(callable.DangerousGetHandle(), args, kwnames, kwvalues);

}
