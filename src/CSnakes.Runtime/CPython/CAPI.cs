using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
using pyoPtr = nint;

internal unsafe partial class CAPI
{
    #region PyObject
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyObject_Repr(MPyOPtr ob);

    /// <summary>
    /// When o is non-NULL, returns a type object corresponding to the object type of object o. 
    /// On failure, raises SystemError and returns NULL. 
    /// This is equivalent to the Python expression type(o). 
    /// This function creates a new strong reference to the return value.
    /// </summary>
    /// <param name="ob">The python object</param>
    /// <returns>A new reference to the Type object for the given Python object</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyObject_Type(MPyOPtr ob);

    /// <summary>
    /// Return 1 if inst is an instance of the class cls or a subclass of cls, or 0 if not. 
    /// On error, returns -1 and sets an exception.
    /// </summary>
    /// <param name="ob">The Python object</param>
    /// <param name="type">The Python type object</param>
    /// <returns></returns>
    [LibraryImport(PythonLibraryName)]
    public static partial int PyObject_IsInstance(MPyOPtr ob, MPyOPtr type);

    [LibraryImport(PythonLibraryName)]
    public static partial int PyObject_IsInstance(MPyOPtr ob, pyoPtr type);


    [LibraryImport(PythonLibraryName)]
    public static partial nint PyObject_GetAttr(MPyOPtr ob, MPyOPtr attr);

    [LibraryImport(PythonLibraryName)]
    public static partial nint PyObject_GetAttr(MPyOPtr ob, pyoPtr attr);

    /// <summary>
    /// Does the object ob have the attr `attr`?
    /// </summary>
    /// <param name="ob">The Python object</param>
    /// <param name="attr">The attribute as a PyUnicode object</param>
    /// <returns>1 on success, 0 if the attr does not exist</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial int PyObject_HasAttr(MPyOPtr ob, MPyOPtr attr);

    [LibraryImport(PythonLibraryName)]
    public static partial int PyObject_HasAttr(MPyOPtr ob, pyoPtr attr);

    /// <summary>
    /// Get the iterator for the given object
    /// </summary>
    /// <param name="ob"></param>
    /// <returns>A new reference to the iterator</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyObject_GetIter(MPyOPtr ob);

    /// <summary>
    /// Get the str(ob) form of the object
    /// </summary>
    /// <param name="ob">The Python object</param>
    /// <returns>A new reference to the string representation</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyObject_Str(MPyOPtr ob);

    /// <summary>
    /// Implements ob[key]
    /// </summary>
    /// <param name="ob"></param>
    /// <param name="key"></param>
    /// <returns>A new reference to the object item if it exists, else NULL</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyObject_GetItem(MPyOPtr ob, MPyOPtr key);

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
    public static partial int PyObject_SetItem(MPyOPtr ob, MPyOPtr key, MPyOPtr value);

    [LibraryImport(PythonLibraryName)]
    public static partial int PyObject_Hash(MPyOPtr ob);


    [LibraryImport(PythonLibraryName)]
    public static partial int PyObject_RichCompareBool(MPyOPtr ob1, MPyOPtr ob2, RichComparisonType comparisonType);

    /// <summary>
    /// Call a callable with no arguments (3.9+)
    /// </summary>
    /// <param name="callable"></param>
    /// <returns>A new reference to the result, or null on failure</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyObject_CallNoArgs(MPyOPtr callable);

    /// <summary>
    /// Call a callable with one argument (3.11+)
    /// </summary>
    /// <param name="callable">Callable object</param>
    /// <param name="arg1">The first argument</param>
    /// <returns>A new reference to the result, or null on failure</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyObject_CallOneArg(MPyOPtr callable, MPyOPtr arg1);


    /// <summary>
    /// Call a callable with many arguments
    /// </summary>
    /// <param name="callable">Callable object</param>
    /// <param name="args">A PyTuple of positional arguments</param>
    /// <param name="kwargs">A PyDict of keyword arguments</param>
    /// <returns>A new reference to the result, or null on failure</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyObject_Call(MPyOPtr callable, pyoPtr args, pyoPtr kwargs);

    [LibraryImport(PythonLibraryName)]
    public static partial nint PyObject_Vectorcall(MPyOPtr callable, pyoPtr* args, nuint nargsf, pyoPtr kwnames);
    #endregion

    #region PyBytes
    [LibraryImport(PythonLibraryName)]
    private static partial byte* PyBytes_AsString(MPyOPtr ob);

    [LibraryImport(PythonLibraryName)]
    private static partial nint PyBytes_Size(MPyOPtr ob);
    #endregion

    #region PyDict
    /// <summary>
    /// Return the number of items in the dictionary as ssize_t
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyDict_Size(MPyOPtr dict);

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
    internal static partial int PyDict_SetItem(MPyOPtr dict, MPyOPtr key, MPyOPtr value);


    /// <summary>
    /// Get the items iterator for the dictionary.
    /// </summary>
    /// <param name="dict">PyDict object</param>
    /// <returns>New reference to the items().</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyDict_Items(MPyOPtr dict);

    [LibraryImport(PythonLibraryName)]
    private static partial int PyDict_Contains(MPyOPtr dict, MPyOPtr key);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyDict_Keys(MPyOPtr dict);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyDict_Values(MPyOPtr dict);
    #endregion

    #region PyFloat
    /// <summary>
    /// Convery a PyFloat to a C double
    /// </summary>
    /// <param name="p"></param>
    /// <returns>The double value</returns>
    [LibraryImport(PythonLibraryName)]
    private static partial double PyFloat_AsDouble(MPyOPtr obj);
    #endregion

    #region PyGen
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyGen_New(MPyOPtr frame);
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
    internal static partial nint PyIter_Next(MPyOPtr iter);
    #endregion

    #region PyList
    /// <summary>
    /// Get a reference to the item at `pos` in the list
    /// </summary>
    /// <param name="obj">The list object</param>
    /// <param name="pos">The position as ssize_t</param>
    /// <returns>Borrowed reference to the list item</returns>
    [LibraryImport(PythonLibraryName)]
    private static partial nint PyList_GetItem(MPyOPtr obj, nint pos);

    /// <summary>
    /// Get the size of the list
    /// </summary>
    /// <param name="obj">PyList object</param>
    /// <returns>The size as ssize_t</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyList_Size(MPyOPtr obj);


    [LibraryImport(PythonLibraryName)]
    public static partial int PyList_SetItem(MPyOPtr obj, nint pos, MPyOPtr o);

    [LibraryImport(PythonLibraryName)]
    public static partial int PyList_SetItem(MPyOPtr obj, nint pos, pyoPtr o);

    #endregion

    #region PyLong
    [LibraryImport(PythonLibraryName)]
    public static partial long PyLong_AsLongLong(MPyOPtr p);

    [LibraryImport(PythonLibraryName)]
    public static partial int PyLong_AsLong(MPyOPtr p);
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
    public static partial int PyMapping_Check(MPyOPtr ob);

    /// <summary>
    /// Get the items iterator for the mapping.
    /// </summary>
    /// <param name="dict">Object that implements the mapping protocol</param>
    /// <returns>New reference to the items().</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PyMapping_Items(MPyOPtr dict);

    [LibraryImport(PythonLibraryName)]
    public static partial nint PyMapping_Keys(MPyOPtr dict);

    [LibraryImport(PythonLibraryName)]
    public static partial nint PyMapping_Values(MPyOPtr dict);

    [LibraryImport(PythonLibraryName)]
    public static partial nint PyMapping_Size(MPyOPtr dict);

    /// <summary>
    /// Return 1 if the mapping object has the key key and 0 otherwise. 
    /// This is equivalent to the Python expression key in o. This function always succeeds.
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    [LibraryImport(PythonLibraryName)]
    public static partial int PyMapping_HasKey(MPyOPtr dict, MPyOPtr key);
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
    public static partial int PySequence_Check(MPyOPtr ob);

    /// <summary>
    /// Return the number of items in the sequence object as ssize_t
    /// </summary>
    /// <param name="seq">Sequence object</param>
    /// <returns>Number of items in the sequence</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PySequence_Size(MPyOPtr seq);

    /// <summary>
    /// Return the ith element of o, or NULL on failure. This is the equivalent of the Python expression o[i].
    /// </summary>
    /// <param name="seq">Sequence object</param>
    /// <param name="index">Index</param>
    /// <returns>New reference to the item or NULL.</returns>
    [LibraryImport(PythonLibraryName)]
    public static partial nint PySequence_GetItem(MPyOPtr seq, nint index);
    #endregion

    #region PyTuple
    [LibraryImport(PythonLibraryName)]
    public static partial int PyTuple_SetItem(MPyOPtr ob, nint pos, MPyOPtr o);

    [LibraryImport(PythonLibraryName)]
    public static partial nint PyTuple_GetItem(MPyOPtr ob, nint pos);

    [LibraryImport(PythonLibraryName)]
    public static partial nint PyTuple_Size(MPyOPtr p);
    #endregion

    #region PyUnicode
    [LibraryImport(PythonLibraryName)]
    public static partial byte* PyUnicode_AsUTF8(MPyOPtr s);
    #endregion
}
