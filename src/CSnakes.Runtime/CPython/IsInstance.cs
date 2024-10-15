namespace CSnakes.Runtime.CPython;
internal unsafe partial class CAPI
{
    #region PyBytes
    public static bool IsBytes(MPyOPtr ob) => IsInstance(ob, _PyBytesType);
    #endregion

    #region PyBuffer
    public static bool IsBuffer(MPyOPtr p) => PyObject_CheckBuffer(p.DangerousGetHandle()) == 1;
    #endregion

    #region PyBool
    public static bool IsPyBool(MPyOPtr p) => p.DangerousGetHandle() == _PyTrue || p.DangerousGetHandle() == _PyFalse;
    public static bool IsPyTrue(MPyOPtr p) => p.DangerousGetHandle() == _PyTrue;
    #endregion

    #region PyDict
    public static bool IsPyDict(MPyOPtr p) => IsInstance(p, _PyDictType);
    #endregion

    #region PyFloat
    public static bool IsPyFloat(MPyOPtr p) => IsInstance(p, _PyFloatType);
    #endregion

    #region PyGenerator
    public static bool IsPyGenerator(MPyOPtr p)
    {
        // TODO : Find a reference to a generator object.
        return HasAttr(p, _NextStr) && HasAttr(p, _SendStr);
    }
    #endregion

    #region PyList
    public static bool IsPyList(MPyOPtr p) => IsInstance(p, _PyListType);
    #endregion

    #region PyLong
    public static bool IsPyLong(MPyOPtr p) => IsInstance(p, _PyLongType);
    #endregion

    #region PyMapping
    public static bool IsPyMappingWithItems(MPyOPtr p) => PyMapping_Check(p) == 1 && PyObject_HasAttr(p, _ItemsStr) == 1;
    #endregion

    #region PyNone
    public static bool IsNone(MPyOPtr o) => _PyNone == o.DangerousGetHandle();
    #endregion

    #region PySequence
    public static bool IsPySequence(MPyOPtr p) => PySequence_Check(p) == 1;
    #endregion

    #region PyTuple
    public static bool IsPyTuple(MPyOPtr p) => IsInstance(p, _PyTupleType);
    #endregion

    #region PyUnicode
    public static bool IsPyUnicode(MPyOPtr p) => IsInstance(p, _PyUnicodeType);
    #endregion
}
