using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    internal static PyObject ConvertFromTuple(ITuple t)
    {
        PyObject[] pyObjects = new PyObject[t.Length];

        for (var i = 0; i < t.Length; i++)
        {
            pyObjects[i] = PyObject.From(t[i]); // NULL->PyNone
        }

        return Pack.CreateTuple(pyObjects);
    }

    internal static ITuple ConvertToTuple(PyObject pyObj, Type destinationType)
    {
        if (!CPythonAPI.IsPyTuple(pyObj))
        {
            // When hitting a nested tuple that is 8, 15, etc. (the point where the CLR tuples nest), the PyObject
            // is not a Tuple anymore, it's a raw value, so we need to convert it to a tuple.
            if (destinationType.GetGenericArguments().Length == 1)
            {
                return (ITuple)Activator.CreateInstance(destinationType, pyObj.As(destinationType.GetGenericArguments()[0]))!;
            }

            throw new InvalidCastException($"Cannot convert {pyObj.GetPythonType()} to a tuple.");
        }
        nint tupleSize = CPythonAPI.PyTuple_Size(pyObj.DangerousGetHandle());

        // We have to convert the Python values to CLR values, as if we just tried As<object>() it would
        // not parse the Python type to a CLR type, only to a new Python type.
        Type[] types = destinationType.GetGenericArguments();
        object?[] clrValues = new object[Math.Min(8, tupleSize)];

        PyObject[] tupleValues = new PyObject[tupleSize];
        for (nint i = 0; i < tupleValues.Length; i++)
        {
            PyObject value = PyObject.Create(CPythonAPI.PyTuple_GetItemWithNewRef(pyObj, i));
            tupleValues[i] = value;
        }

        // If we have more than 8 python values, we are going to have a nested tuple, which we have to unpack.
        if (tupleValues.Length > 8)
        {
            // We are hitting nested tuples here, which will be treated in a different way.
            IEnumerable<object?> firstSeven = tupleValues.Take(7).Select((p, i) => p.As(types[i]));

            // Get the rest of the values and convert them to a nested tuple.
            IEnumerable<PyObject> rest = tupleValues.Skip(7);

            // Back to a Python tuple.
            using PyObject pyTuple = Pack.CreateTuple(rest.ToArray());

            // Use the decoder pipeline to decode the nested tuple (and its values).
            // We do this because that means if we have nested nested tuples, they'll be decoded as well.
            object? nestedTuple = pyTuple.As(types[7]);

            // Append our nested tuple to the first seven values.
            clrValues = [.. firstSeven, nestedTuple];
        }
        else
        {
            for (var i = 0; i < tupleValues.Length; i++)
            {
                clrValues[i] = tupleValues[i].As(types[i]);
                // Dispose of the Python object created by PyTuple_GetItem earlier in this method.
                tupleValues[i].Dispose();
            }
        }

        if (!knownDynamicTypes.TryGetValue(destinationType, out DynamicTypeInfo? typeInfo))
        {
            ConstructorInfo ctor = destinationType.GetConstructor(destinationType.GetGenericArguments())
                ?? throw new InvalidOperationException($"Could not find a constructor for {destinationType}");
            typeInfo = new(ctor);
            knownDynamicTypes[destinationType] = typeInfo;
        }

        return (ITuple)typeInfo.ReturnTypeConstructor.Invoke([.. clrValues]);
    }

    internal static KeyValuePair<TKey, TValue> ConvertToKeyValuePair<TKey, TValue>(PyObject pyObj)
    {
        if (!CPythonAPI.IsPyTuple(pyObj))
        {
            throw new InvalidCastException($"Cannot convert {pyObj.GetPythonType()} to a keyvaluepair.");
        }
        if (CPythonAPI.PyTuple_Size(pyObj.DangerousGetHandle()) != 2)
        {
            throw new InvalidDataException("Tuple must have exactly 2 elements to be converted to a KeyValuePair.");
        }

        using PyObject key = PyObject.Create(CPythonAPI.PyTuple_GetItemWithNewRef(pyObj, 0));
        using PyObject value = PyObject.Create(CPythonAPI.PyTuple_GetItemWithNewRef(pyObj, 1));

        return new KeyValuePair<TKey, TValue>(key.As<TKey>(), value.As<TValue>());
    }
}
