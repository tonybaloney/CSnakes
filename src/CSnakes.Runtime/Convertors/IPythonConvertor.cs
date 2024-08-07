using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Convertors;

internal interface IPythonConvertor
{
    public bool CanEncode(Type type);

    public bool CanDecode(PyObject objectType, Type targetType);

    public bool TryEncode(object value, out PyObject? result);

    public bool TryDecode(PyObject pyObj, out object? value);
}