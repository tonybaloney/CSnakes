using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Converters;

internal interface IPythonConvertor<T>
{
    public bool CanEncode(Type type);

    public bool TryEncode(T value, out PyObject? result);

    public bool CanDecode(PyObject objectType, Type targetType);

    public bool TryDecode(PyObject pyObj, out T? value);
}
