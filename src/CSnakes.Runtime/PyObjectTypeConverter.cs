using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CSnakes.Runtime;
internal class PyObjectTypeConverter : TypeConverter
{
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (value is not PyObject pyObject)
        {
            throw new NotSupportedException();
        }
        if (pyObject.IsInvalid)
        {
            throw new NullReferenceException("Python object safehandle is invalid");
        }

        nint handle = pyObject.DangerousGetHandle();
        if (destinationType == typeof(string) && CPythonAPI.IsPyUnicode(handle))
        {
            return CPythonAPI.PyUnicode_AsUTF8(handle);
        }

        if (destinationType == typeof(long) && CPythonAPI.IsPyLong(handle))
        {
            return CPythonAPI.PyLong_AsLongLong(handle);
        }

        if (destinationType == typeof(int) && CPythonAPI.IsPyLong(handle))
        {
            return CPythonAPI.PyLong_AsLongLong(handle);
        }

        if (destinationType == typeof(bool) && CPythonAPI.IsPyBool(handle))
        {
            return CPythonAPI.IsPyTrue(handle);
        }

        if (destinationType == typeof(double) && CPythonAPI.IsPyFloat(handle))
        {
            return CPythonAPI.PyFloat_AsDouble(handle);
        }

        if (destinationType.IsGenericType)
        {
            if (destinationType.IsAssignableTo(typeof(IEnumerable)) && CPythonAPI.IsPyDict(handle))
            {
                return ConvertToDictionary(pyObject, destinationType, context, culture);
            }

            if (destinationType.IsAssignableTo(typeof(IEnumerable)) && CPythonAPI.IsPyList(handle))
            {
                return ConvertToList(pyObject, destinationType, context, culture);
            }

            if (destinationType.IsAssignableTo(typeof(ITuple)))
            {
                if (CPythonAPI.IsPyTuple(handle))
                {
                    return ConvertToTuple(context, culture, pyObject, destinationType);
                }

                var tupleTypes = destinationType.GetGenericArguments();
                if (tupleTypes.Length > 1)
                {
                    throw new InvalidCastException($"The type is a tuple with more than one generic argument, but the underlying Python type is not a tuple. Destination Type: {destinationType}");
                }

                var convertedValue = ConvertTo(context, culture, pyObject, tupleTypes[0]);
                return Activator.CreateInstance(destinationType, convertedValue);
            }
        }

        throw new InvalidCastException($"Attempting to cast {destinationType} from {pyObject.Type()}");
    }

    private object? ConvertToDictionary(PyObject pyObject, Type destinationType, ITypeDescriptorContext? context, CultureInfo? culture)
    {
        PyObject items = new PyObject(CPythonAPI.PyDict_Items(pyObject.DangerousGetHandle()));
        Type item1Type = destinationType.GetGenericArguments()[0];
        Type item2Type = destinationType.GetGenericArguments()[1];
        Type dictType = typeof(Dictionary<,>).MakeGenericType(item1Type, item2Type);
        var dict = (IDictionary)Activator.CreateInstance(dictType)!;
        nint itemsLength = CPythonAPI.PyList_Size(items.DangerousGetHandle());

        for (nint i = 0; i < itemsLength; i++)
        {
            PyObject item = new PyObject(CPythonAPI.PyList_GetItem(items.DangerousGetHandle(), i));

            PyObject item1 = new PyObject(CPythonAPI.PyTuple_GetItem(item.DangerousGetHandle(), 0));
            PyObject item2 = new PyObject(CPythonAPI.PyTuple_GetItem(item.DangerousGetHandle(), 1));

            var convertedItem1 = AsManagedObject(item1Type, item1, context, culture);
            var convertedItem2 = AsManagedObject(item2Type, item2, context, culture);

            dict.Add(convertedItem1!, convertedItem2);
        }

        Type returnType = typeof(ReadOnlyDictionary<,>).MakeGenericType(item1Type, item2Type);
        return Activator.CreateInstance(returnType, dict);
    }

    private object? ConvertToTuple(ITypeDescriptorContext? context, CultureInfo? culture, PyObject pyObj, Type destinationType)
    {
        var tuplePtr = pyObj.DangerousGetHandle();

        // We have to convert the Python values to CLR values, as if we just tried As<object>() it would
        // not parse the Python type to a CLR type, only to a new Python type.
        Type[] types = destinationType.GetGenericArguments();
        object?[] clrValues;

        var tupleValues = new List<PyObject>();
        for (nint i = 0; i < CPythonAPI.PyTuple_Size(tuplePtr); i++)
        {
            // TODO: Inspect GetItem returns borrowed or new reference.
            tupleValues.Add(new PyObject(CPythonAPI.PyTuple_GetItem(tuplePtr, i)));
        }

        // If we have more than 8 python values, we are going to have a nested tuple, which we have to unpack.
        if (tupleValues.Count > 8)
        {
            // We are hitting nested tuples here, which will be treated in a different way.
            object?[] firstSeven = tupleValues.Take(7).Select((p, i) => AsManagedObject(types[i], p, context, culture)).ToArray();

            // Get the rest of the values and convert them to a nested tuple.
            IEnumerable<PyObject> rest = tupleValues.Skip(7);

            // Back to a Python tuple.
            PyObject pyTuple = PyTuple.CreateTuple(rest);

            // Use the decoder pipeline to decode the nested tuple (and its values).
            // We do this because that means if we have nested nested tuples, they'll be decoded as well.
            object? nestedTuple = AsManagedObject(types[7], pyTuple, context, culture);

            // Append our nested tuple to the first seven values.
            clrValues = [.. firstSeven, nestedTuple];
        }
        else
        {
            clrValues = tupleValues.Select((p, i) => AsManagedObject(types[i], p, context, culture)).ToArray();
        }

        ConstructorInfo ctor = destinationType.GetConstructors().First(c => c.GetParameters().Length == clrValues.Length);
        return (ITuple)ctor.Invoke(clrValues);
    }

    private object? AsManagedObject(Type type, PyObject p, ITypeDescriptorContext? context, CultureInfo? culture)
    {
        return ConvertTo(context, culture, p, type);
    }

    private object? ConvertToList(PyObject pyObject, Type destinationType, ITypeDescriptorContext? context, CultureInfo? culture)
    {
        Type genericArgument = destinationType.GetGenericArguments()[0];
        Type listType = typeof(List<>).MakeGenericType(genericArgument);

        IList list = (IList)Activator.CreateInstance(listType)!;
        for (var i = 0; i < CPythonAPI.PyList_Size(pyObject.DangerousGetHandle()); i++)
        {
            var item = new PyObject(CPythonAPI.PyList_GetItem(pyObject.DangerousGetHandle(), i));
            list.Add(AsManagedObject(genericArgument, item, context, culture));
        }

        return list;
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        value switch
        {
            string str => new PyObject(CPythonAPI.AsPyUnicodeObject(str)),
            long l => new PyObject(CPythonAPI.PyLong_FromLongLong(l)),
            int i => new PyObject(CPythonAPI.PyLong_FromLong(i)),
            bool b => new PyObject(CPythonAPI.PyBool_FromLong(b ? 1 : 0)),
            double d => new PyObject(CPythonAPI.PyFloat_FromDouble(d)),
            IDictionary dictionary => ConvertFromDictionary(context, culture, dictionary),
            ITuple t => ConvertFromTuple(context, culture, t),
            IEnumerable e => ConvertFromList(context, culture, e),
            null => new PyObject(CPythonAPI.GetNone()),
            _ => base.ConvertFrom(context, culture, value)
        };

    private PyObject ConvertFromDictionary(ITypeDescriptorContext? context, CultureInfo? culture, IDictionary dictionary)
    {
        var pyDict = CPythonAPI.PyDict_New();
        if (pyDict == IntPtr.Zero)
        {
            throw new Exception("Failed to create dictionary");
        }

        foreach (DictionaryEntry kvp in dictionary)
        {
            int hresult = CPythonAPI.PyDict_SetItem(pyDict, ToPython(kvp.Key, context, culture).DangerousGetHandle(), ToPython(kvp.Value, context, culture).DangerousGetHandle());
            if (hresult == -1)
            {
                throw new Exception("Failed to set item in dictionary");
            }
        }

        return new PyObject(pyDict);
    }

    private PyObject ConvertFromList(ITypeDescriptorContext? context, CultureInfo? culture, IEnumerable e)
    {
        List<PyObject> list = [];
        foreach (var item in e)
        {
            var converted = ConvertFrom(context, culture, item);
            list.Add((PyObject)converted!);
        }

        var pyList = CPythonAPI.PyList_New(list.Count);
        for (var i = 0; i < list.Count; i++)
        {
            int hresult = CPythonAPI.PyList_SetItem(pyList, i, list[i].DangerousGetHandle());
            if (hresult == -1)
            {
                throw new Exception("Failed to set item in list");
            }
        }

        return new PyObject(pyList);
    }

    private PyObject ConvertFromTuple(ITypeDescriptorContext? context, CultureInfo? culture, ITuple t)
    {
        List<PyObject> pyObjects = [];

        for (var i = 0; i < t.Length; i++)
        {
            var currentValue = t[i];
            if (currentValue is null)
            {
                // TODO: handle null values
            }
            pyObjects.Add(ToPython(currentValue, context, culture));
        }

        return PyTuple.CreateTuple(pyObjects);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType) =>
        destinationType is not null && (
            destinationType == typeof(string) ||
            destinationType == typeof(long) ||
            destinationType == typeof(int) ||
            destinationType == typeof(bool) ||
            destinationType == typeof(double) ||
            destinationType.IsGenericType
        );

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType is not null && (
            sourceType == typeof(string) ||
            sourceType == typeof(long) ||
            sourceType == typeof(int) ||
            sourceType == typeof(bool) ||
            sourceType == typeof(double) ||
            sourceType.IsGenericType
        );

    private PyObject ToPython(object o, ITypeDescriptorContext? context, CultureInfo? culture)
    {
        var result = ConvertFrom(context, culture, o);

        if (result is null)
        {
            throw new NotImplementedException();
        }

        return (PyObject)result;
    }
}
