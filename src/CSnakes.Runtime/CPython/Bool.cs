﻿using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    private static nint PyBoolType = IntPtr.Zero;
    private static nint Py_True = IntPtr.Zero;
    private static nint Py_False = IntPtr.Zero;

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyBool_FromLong(long value);

    public static bool IsPyBool(nint p)
    {
        return ((PyObjectStruct*)p)->Type() == PyBoolType;
    }

    public static bool IsPyTrue(nint p)
    {
        return p == Py_True;
    }
}