using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython.Unmanaged;
using pyoPtr = nint;

internal unsafe partial class CAPI
{

    #region PyObject
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyObject_Repr(pyoPtr ob);

    /// <summary>
    /// When o is non-NULL, returns a type object corresponding to the object type of object o. 
    /// On failure, raises SystemError and returns NULL. 
    /// This is equivalent to the Python expression type(o). 
    /// This function creates a new strong reference to the return value.
    /// </summary>
    /// <param name="ob">The python object</param>
    /// <returns>A new reference to the Type object for the given Python object</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyObject_Type(pyoPtr ob);

    /// <summary>
    /// Return 1 if inst is an instance of the class cls or a subclass of cls, or 0 if not. 
    /// On error, returns -1 and sets an exception.
    /// </summary>
    /// <param name="ob">The Python object</param>
    /// <param name="type">The Python type object</param>
    /// <returns></returns>
    [LibraryImport(PythonLibraryName)]
    public static partial int PyObject_IsInstance(pyoPtr ob, pyoPtr type);


    [LibraryImport(PythonLibraryName)]
    public static partial nint PyObject_GetAttr(pyoPtr ob, pyoPtr attr);

    /// <summary>
    /// Does the object ob have the attr `attr`?
    /// </summary>
    /// <param name="ob">The Python object</param>
    /// <param name="attr">The attribute as a PyUnicode object</param>
    /// <returns>1 on success, 0 if the attr does not exist</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial int PyObject_HasAttr(pyoPtr ob, pyoPtr attr);

    /// <summary>
    /// Get the iterator for the given object
    /// </summary>
    /// <param name="ob"></param>
    /// <returns>A new reference to the iterator</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyObject_GetIter(pyoPtr ob);

    /// <summary>
    /// Get the str(ob) form of the object
    /// </summary>
    /// <param name="ob">The Python object</param>
    /// <returns>A new reference to the string representation</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyObject_Str(pyoPtr ob);

    /// <summary>
    /// Implements ob[key]
    /// </summary>
    /// <param name="ob"></param>
    /// <param name="key"></param>
    /// <returns>A new reference to the object item if it exists, else NULL</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyObject_GetItem(pyoPtr ob, pyoPtr key);

    /// <summary>
    /// Set ob[key] = value
    /// Returns -1 on error and sets exception.
    /// Does not steal a reference to value on success.
    /// </summary>
    /// <param name="ob"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [LibraryImport(PythonLibraryName)]
    public static partial int PyObject_SetItem(pyoPtr ob, pyoPtr key, pyoPtr value);

    [LibraryImport(PythonLibraryName)]
    public static partial int PyObject_Hash(pyoPtr ob);


    public enum RichComparisonType : int
    {
        LessThan = 0,
        LessThanEqual = 1,
        Equal = 2,
        NotEqual = 3,
        GreaterThan = 4,
        GreaterThanEqual = 5
    }

    [LibraryImport(PythonLibraryName)]
    public static partial int PyObject_RichCompareBool(pyoPtr ob1, pyoPtr ob2, RichComparisonType comparisonType);

    /// <summary>
    /// Call a callable with no arguments (3.9+)
    /// </summary>
    /// <param name="callable"></param>
    /// <returns>A new reference to the result, or null on failure</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyObject_CallNoArgs(pyoPtr callable);

    /// <summary>
    /// Call a callable with one argument (3.11+)
    /// </summary>
    /// <param name="callable">Callable object</param>
    /// <param name="arg1">The first argument</param>
    /// <returns>A new reference to the result, or null on failure</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyObject_CallOneArg(pyoPtr callable, pyoPtr arg1);


    /// <summary>
    /// Call a callable with many arguments
    /// </summary>
    /// <param name="callable">Callable object</param>
    /// <param name="args">A PyTuple of positional arguments</param>
    /// <param name="kwargs">A PyDict of keyword arguments</param>
    /// <returns>A new reference to the result, or null on failure</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyObject_Call(pyoPtr callable, pyoPtr args, pyoPtr kwargs);

    [LibraryImport(PythonLibraryName)]
    public static partial nint PyObject_Vectorcall(pyoPtr callable, pyoPtr* args, nuint nargsf, pyoPtr kwnames);
    #endregion

    #region PyBool
    /// <summary>
    /// Convert a Int32 to a PyBool Object
    /// </summary>
    /// <param name="value">Numeric value (0 or 1)</param>
    /// <returns>New reference to a PyBool Object</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyBool_FromLong(nint value);
    #endregion

    #region PyBuffer
    [Flags]
    public enum PyBUF : int
    {
        Simple = 0,
        Writable = 0x1,
        Format = 0x4,
        ND = 0x8,
        Strides = 0x10 | ND,
        CContiguous = 0x20 | Strides,
        FContiguous = 0x40 | Strides,
        AnyContiguous = 0x80 | Strides,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PyBuffer
    {
        public nint buf;
        public nint obj;
        public nint len;
        public nint itemsize;
        public int @readonly;
        public int ndim;
        public byte* format;
        public nint* shape;
        public nint* strides;
        public nint* suboffsets;
        public nint* @internal;
    }

    [LibraryImport(PythonLibraryName)]
    public static partial int PyObject_CheckBuffer(pyoPtr ob);

    [LibraryImport(PythonLibraryName)]
    public static partial int PyObject_GetBuffer(pyoPtr ob, PyBuffer* view, int flags);

    [LibraryImport(PythonLibraryName)]
    public static partial void PyBuffer_Release(PyBuffer* view);
    #endregion

    #region PyBytes
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyBytes_FromStringAndSize(byte* v, nint len);

    [LibraryImport(PythonLibraryName)]
    public static partial byte* PyBytes_AsString(pyoPtr ob);

    [LibraryImport(PythonLibraryName)]
    public static partial nint PyBytes_Size(pyoPtr ob);
    #endregion

    #region PyDict
    /// <summary>
    /// Create a new dictionary object.
    /// </summary>
    /// <returns>Return a new empty dictionary, or NULL on failure.</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyDict_New();


    /// <summary>
    /// Return the number of items in the dictionary as ssize_t
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyDict_Size(pyoPtr dict);


    /// <summary>
    /// Return the object from dictionary p which has a key `key`. 
    /// Return NULL if the key key is not present, but without setting an exception.
    /// </summary>
    /// <param name="dict">Dictionary Object</param>
    /// <param name="key">Key Object</param>
    /// <returns>Borrowed reference.</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyDict_GetItem(pyoPtr dict, pyoPtr key);

    /// <summary>
    /// Insert val into the dictionary p with a key of key. 
    /// key must be hashable; if it isn’t, TypeError will be raised.  
    /// This function adds a new reference to key and value if successful.
    /// </summary>
    /// <param name="dict">PyDict object</param>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <returns>Return 0 on success or -1 on failure.</returns>

    [LibraryImport(PythonLibraryName)]
    public static partial int PyDict_SetItem(pyoPtr dict, pyoPtr key, pyoPtr val);

    /// <summary>
    /// Get the items iterator for the dictionary.
    /// </summary>
    /// <param name="dict">PyDict object</param>
    /// <returns>New reference to the items().</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyDict_Items(pyoPtr dict);

    [LibraryImport(PythonLibraryName)]
    public static partial int PyDict_Contains(pyoPtr dict, pyoPtr key);

    [LibraryImport(PythonLibraryName)]
    public static partial nint PyDict_Keys(pyoPtr dict);

    [LibraryImport(PythonLibraryName)]
    public static partial nint PyDict_Values(pyoPtr dict);
    #endregion

    #region PyModule
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyModule_GetDict(pyoPtr module);
    #endregion

    #region PyErr
    /// <summary>
    /// Keep this function private. Use IsPyErrOccurred() instead.
    /// It returns a borrowed reference to the exception object. USe PyErr_Fetch if you need the exception.
    /// </summary>
    /// <returns></returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyErr_Occurred();

    [LibraryImport(PythonLibraryName)]
    public static partial void PyErr_Clear();

    // https://docs.python.org/3.10/c-api/exceptions.html#c.PyErr_Fetch
    /// <summary>
    /// Retrieve the error indicator into three variables whose addresses are passed. 
    /// If the error indicator is not set, set all three variables to NULL. 
    /// If it is set, it will be cleared and you own a reference to each object retrieved. 
    /// The value and traceback object may be NULL even when the type object is not.
    /// </summary>
    /// <param name="ptype"></param>
    /// <param name="pvalue"></param>
    /// <param name="ptraceback"></param>
    [LibraryImport(PythonLibraryName)]
    public static partial void PyErr_Fetch(out nint ptype, out nint pvalue, out nint ptraceback);
    #endregion

    #region PyFloat

    /// <summary>
    /// Create a PyFloat from the C double
    /// </summary>
    /// <param name="v"></param>
    /// <returns>A new reference to the float object</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyFloat_FromDouble(double value);


    /// <summary>
    /// Convery a PyFloat to a C double
    /// </summary>
    /// <param name="p"></param>
    /// <returns>The double value</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial double PyFloat_AsDouble(pyoPtr obj);
    #endregion

    #region PyGen
    [LibraryImport(PythonLibraryName)]
    public static partial pyoPtr PyGen_New(nint frame);
    #endregion

    #region PyGILState
    /// <summary>
    /// Ensure (hold) GIL and return a GIL state handle (ptr)
    /// Handles are not memory managed by Python. Do not free them.
    /// </summary>
    /// <returns>A GIL state handle</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyGILState_Ensure();

    /// <summary>
    /// Release the GIL with the given state handle
    /// </summary>
    /// <param name="state">GIL State handle</param>
    [LibraryImport(PythonLibraryName)]
    public static partial void PyGILState_Release(nint state);

    /// <summary>
    /// Check the GIL is held by the current thread
    /// </summary>
    /// <returns>1 if held, 0 if not.</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial int PyGILState_Check();
    #endregion

    #region PyEval

    [LibraryImport(PythonLibraryName)]
    public static partial void PyEval_ReleaseLock();

    [LibraryImport(PythonLibraryName)]
    public static partial nint PyEval_SaveThread();
    #endregion

    #region PyImport
    /// <summary>
    /// Import and return a reference to the module `name`
    /// </summary>
    /// <param name="name">The name of the module as a PyUnicode object.</param>
    /// <returns>A new reference to module `name`</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyImport_Import(pyoPtr name);

    [LibraryImport(PythonLibraryName)]
    public static partial nint PyImport_AddModule(pyoPtr name);
    #endregion

    #region Py
    /*
    [LibraryImport(PythonLibraryName)]
    public static partial byte* Py_GetVersion();
    */

    [LibraryImport(PythonLibraryName)]
    public static partial void Py_Initialize();

    [LibraryImport(PythonLibraryName)]
    public static partial void Py_Finalize();

    [LibraryImport(PythonLibraryName)]
    public static partial int Py_IsInitialized();

    /*
    [LibraryImport(PythonLibraryName, EntryPoint = "Py_SetPath")]
    internal static partial void Py_SetPath_UCS2_UTF16([MarshalAs(UnmanagedType.LPWStr)] string path);

    [LibraryImport(PythonLibraryName, EntryPoint = "Py_SetPath", StringMarshallingCustomType = typeof(Utf32StringMarshaller), StringMarshalling = StringMarshalling.Custom)]
    internal static partial void Py_SetPath_UCS4_UTF32(string path);
    */


    [LibraryImport(PythonLibraryName)]
    public static partial void Py_DecRef(pyoPtr ob);

    [LibraryImport(PythonLibraryName)]
    public static partial void Py_IncRef(pyoPtr ob);
    #endregion

    #region PyIter
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
    public static partial nint PyIter_Next(pyoPtr iter);
    #endregion

    #region PyList
    /// <summary>
    /// Create a new list of length `size`
    /// </summary>
    /// <param name="size">Size as ssize_t</param>
    /// <returns>A new reference to the list</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyList_New(nint size);

    /// <summary>
    /// Get the size of the list
    /// </summary>
    /// <param name="obj">PyList object</param>
    /// <returns>The size as ssize_t</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyList_Size(pyoPtr obj);

    /// <summary>
    /// Get a reference to the item at `pos` in the list
    /// </summary>
    /// <param name="obj">The list object</param>
    /// <param name="pos">The position as ssize_t</param>
    /// <returns>Borrowed reference to the list item</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyList_GetItem(pyoPtr obj, nint pos);


    [LibraryImport(PythonLibraryName)]
    public static partial int PyList_SetItem(pyoPtr obj, nint pos, pyoPtr o);
    #endregion

    #region PyLong
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyLong_FromLong(int v);

    [LibraryImport(PythonLibraryName)]
    public static partial nint PyLong_FromLongLong(long v);

    [LibraryImport(PythonLibraryName)]
    public static partial long PyLong_AsLongLong(pyoPtr p);

    [LibraryImport(PythonLibraryName)]
    public static partial int PyLong_AsLong(pyoPtr p);

    [LibraryImport(PythonLibraryName)]
    public static partial nint PyLong_FromUnicodeObject(pyoPtr unicode, int @base);
    #endregion

    #region PyMapping
    /// <summary>
    /// Return 1 if the object provides the mapping protocol or supports slicing, and 0 otherwise. 
    /// Note that it returns 1 for Python classes with a __getitem__() method, since in general 
    /// it is impossible to determine what type of keys the class supports. This function always succeeds.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    [LibraryImport(PythonLibraryName)]
    public static partial int PyMapping_Check(pyoPtr ob);

    /// <summary>
    /// Get the items iterator for the mapping.
    /// </summary>
    /// <param name="dict">Object that implements the mapping protocol</param>
    /// <returns>New reference to the items().</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyMapping_Items(pyoPtr dict);

    [LibraryImport(PythonLibraryName)]
    public static partial nint PyMapping_Keys(pyoPtr dict);

    [LibraryImport(PythonLibraryName)]
    public static partial nint PyMapping_Values(pyoPtr dict);

    [LibraryImport(PythonLibraryName)]
    public static partial nint PyMapping_Size(pyoPtr dict);

    /// <summary>
    /// Return 1 if the mapping object has the key key and 0 otherwise. 
    /// This is equivalent to the Python expression key in o. This function always succeeds.
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    [LibraryImport(PythonLibraryName)]
    public static partial int PyMapping_HasKey(pyoPtr dict, pyoPtr key);
    #endregion

    #region PyRun
    [StructLayout(LayoutKind.Sequential)]
    public struct PyCompilerFlags
    {
        public int cf_flags;  /* bitmask of CO_xxx flags relevant to future */
        public int cf_feature_version;  /* minor Python version (PyCF_ONLY_AST) */
    }

    public enum Token : int
    {
        Single = 256,
        File = 257, /* Py_file_input */
        Eval = 258
    }


    [LibraryImport(PythonLibraryName)]
    public static partial int PyRun_StringFlags(byte* source, Token start, pyoPtr globals, pyoPtr locals, PyCompilerFlags* flags);
    #endregion

    #region PyTuple
    /// <summary>
    /// Create a new tuple of size `size` as ssize_t
    /// </summary>
    /// <param name="size"></param>
    /// <returns>A new reference to the tuple object, or NULL on failure.</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyTuple_New(nint size);


    [LibraryImport(PythonLibraryName)]
    public static partial int PyTuple_SetItem(pyoPtr ob, nint pos, pyoPtr o);

    [LibraryImport(PythonLibraryName)]
    public static partial nint PyTuple_GetItem(pyoPtr ob, nint pos);

    [LibraryImport(PythonLibraryName)]
    public static partial nint PyTuple_Size(pyoPtr p);
    #endregion

    #region PyUnicode
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyUnicode_DecodeUTF16(char* str, nint size, pyoPtr errors, pyoPtr byteorder);

    [LibraryImport(PythonLibraryName)]
    public static partial byte* PyUnicode_AsUTF8(pyoPtr s);
    #endregion

    #region PySequence
    /// <summary>
    /// Return 1 if the object provides the sequence protocol, and 0 otherwise.
    /// Note that it returns 1 for Python classes with a __getitem__() method, 
    /// unless they are dict subclasses, since in general it is impossible to determine 
    /// what type of keys the class supports. This function always succeeds.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    [LibraryImport(PythonLibraryName)]
    public static partial int PySequence_Check(pyoPtr ob);

    /// <summary>
    /// Return the number of items in the sequence object as ssize_t
    /// </summary>
    /// <param name="seq">Sequence object</param>
    /// <returns>Number of items in the sequence</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PySequence_Size(pyoPtr seq);

    /// <summary>
    /// Return the ith element of o, or NULL on failure. This is the equivalent of the Python expression o[i].
    /// </summary>
    /// <param name="seq">Sequence object</param>
    /// <param name="index">Index</param>
    /// <returns>New reference to the item or NULL.</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PySequence_GetItem(pyoPtr seq, nint index);
    #endregion

}
