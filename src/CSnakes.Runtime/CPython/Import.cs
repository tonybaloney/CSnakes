﻿using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    /// <summary>
    /// Import a module and return a reference to it.
    /// </summary>
    /// <param name="name">The module name</param>
    /// <returns>A new reference to module `name`</returns>
    internal static PyObject Import(string name)
    {
        nint pyName = AsPyUnicodeObject(name);
        nint module = PyImport_Import(pyName);
        Py_DecRefRaw(pyName);
        return PyObject.Create(module);
    }

    internal static PyObject ReloadModule(PyObject module)
    {
        nint reloaded = PyImport_ReloadModule(module);
        return PyObject.Create(reloaded);
    }

    protected static nint GetBuiltin(string name)
    {
        nint pyName = AsPyUnicodeObject("builtins");
        nint pyAttrName = AsPyUnicodeObject(name);
        nint module = PyImport_Import(pyName);
        nint attr = PyObject_GetAttrRaw(module, pyAttrName);
        if (attr == IntPtr.Zero)
        {
            throw PyObject.ThrowPythonExceptionAsClrException();
        }
        Py_DecRefRaw(pyName);
        Py_DecRefRaw(pyAttrName);
        return attr;
    } 

    /// <summary>
    /// Import and return a reference to the module `name`
    /// </summary>
    /// <param name="name">The name of the module as a PyUnicode object.</param>
    /// <returns>A new reference to module `name`</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyImport_Import(nint name);


    /// <summary>
    /// Reload a module. Return a new reference to the reloaded module, or NULL with an exception set on failure (the module still exists in this case).
    /// </summary>
    /// <param name="module"></param>
    /// <returns></returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyImport_ReloadModule(PyObject module);

}
