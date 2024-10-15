namespace CSnakes.Runtime.CPython.Unmanaged;
using pyoPtr = nint;

internal unsafe partial class CAPI
{
    internal void InitializeTypesAndStaticObjects()
    {
        _PyUnicodeType = PyObject_Type(AsPyUnicodeObject(string.Empty));
        _PyTrue = PyBool_FromLong(1);
        _PyFalse = PyBool_FromLong(0);
        _PyBoolType = PyObject_Type(_PyTrue);
        _PyEmptyTuple = PyTuple_New(0);
        _PyTupleType = PyObject_Type(_PyEmptyTuple);
        _PyFloatType = PyObject_Type(PyFloat_FromDouble(0.0));
        _PyLongType = PyObject_Type(PyLong_FromLongLong(0));
        _PyListType = PyObject_Type(PyList_New(0));
        _PyDictType = PyObject_Type(PyDict_New());
        _PyBytesType = PyObject_Type(ByteSpanToPyBytes(new byte[] { }));
        _ItemsStr = AsPyUnicodeObject("items");
        _NextStr = AsPyUnicodeObject("__next__");
        _SendStr = AsPyUnicodeObject("send");
        _PyNone = GetBuiltin("None");
    }


    #region PyBool
    protected static pyoPtr _PyBoolType = IntPtr.Zero;
    protected static pyoPtr _PyTrue = IntPtr.Zero;
    protected static pyoPtr _PyFalse = IntPtr.Zero;

    public static pyoPtr PtrToPyBoolType => _PyBoolType;
    public static pyoPtr PtrToPyTrue => _PyTrue;
    public static pyoPtr PtrToPyFalse => _PyFalse;
    #endregion

    #region PyByes
    protected static pyoPtr _PyBytesType = IntPtr.Zero;

    public static pyoPtr PtrToPyBytesType => _PyBytesType;
    #endregion

    #region PyDict
    protected static pyoPtr _PyDictType = IntPtr.Zero;

    public static pyoPtr PtrToPyDictType => _PyDictType;
    #endregion

    #region PyFloat
    protected static pyoPtr _PyFloatType = IntPtr.Zero;

    public static pyoPtr PtrToPyFloatType => _PyFloatType;
    #endregion

    #region PyList
    protected static pyoPtr _PyListType = IntPtr.Zero;

    public static pyoPtr PtrToPyListType => _PyListType;
    #endregion

    #region PyLong
    protected static nint _PyLongType = IntPtr.Zero;

    public static pyoPtr PtrToPyLongType => _PyLongType;
    #endregion

    #region PyUnicode "items", "__next__", "send"
    protected static pyoPtr _ItemsStr = IntPtr.Zero;
    protected static pyoPtr _NextStr = IntPtr.Zero;
    protected static pyoPtr _SendStr = IntPtr.Zero;

    public static pyoPtr PtrToItemsStr => _ItemsStr;
    public static pyoPtr PtrToNextStr => _ItemsStr;
    public static pyoPtr PtrToSendStr => _ItemsStr;
    #endregion

    #region PyNone
    protected static pyoPtr _PyNone = IntPtr.Zero;

    public static pyoPtr PtrToPyNone => _PyNone;
    #endregion

    #region PyTuple
    protected static pyoPtr _PyTupleType = IntPtr.Zero;
    protected static pyoPtr _PyEmptyTuple = IntPtr.Zero;
    public static pyoPtr PtrToPyTupleType => _PyTupleType;
    #endregion

    #region PyUnicode
    protected static pyoPtr _PyUnicodeType = IntPtr.Zero;

    public static pyoPtr PtrToPyUnicodeType => _PyUnicodeType;
    #endregion
}
