using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Convertors;

public sealed class ListConvertor<TKey> : IPythonConvertor
{
    public bool CanDecode(PyObject objectType, Type targetType) =>
        targetType.IsGenericType && typeof(IEnumerable<TKey>).IsAssignableFrom(targetType);

    public bool CanEncode(Type type) =>
        type.IsGenericType && typeof(IEnumerable<TKey>).IsAssignableFrom(type);

    public bool TryEncode(object value, out PyObject? result)
    {
        var coll = (IEnumerable<TKey>)value;
        var list = coll.ToArray();

        var pyList = CPythonAPI.PyList_New(coll.Count());
        for (var i = 0; i < coll.Count(); i++)
        {
            int hresult = CPythonAPI.PyList_SetItem(pyList, i, list[i].ToPython().DangerousGetHandle());
            if (hresult == -1)
            {
                result = null;
                // TODO: Forward exception
                return false;
            }
        }

        result = new PyObject(pyList);
        return true;
    }

    public bool TryDecode(PyObject value, out object? result)
    {
        // Check is list
        if (!CPythonAPI.IsPyList(value.DangerousGetHandle()))
        {
            result = null;
            // TODO: Raise something to say its the wrong type
            return false;
        }

        var list = new List<TKey>();
        for (var i = 0; i < CPythonAPI.PyList_Size(value.DangerousGetHandle()); i++)
        {
            var item = new PyObject(CPythonAPI.PyList_GetItem(value.DangerousGetHandle(), i));
            list.Add(item.As<TKey>());
        }

        result = list;
        return true;
    }
}
