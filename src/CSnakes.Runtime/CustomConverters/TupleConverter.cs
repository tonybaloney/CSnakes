using Python.Runtime;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CSnakes.Runtime.CustomConverters;

public class TupleConverter : IPyObjectEncoder, IPyObjectDecoder
{
    public bool CanDecode(PyType objectType, Type targetType) => typeof(ITuple).IsAssignableFrom(targetType);

    public bool CanEncode(Type type) => typeof(ITuple).IsAssignableFrom(type);

    public bool TryDecode<T>(PyObject pyObj, out T value)
    {
        IEnumerable<PyObject> pyValues = pyObj.AsEnumerable<PyObject>();
        // We have to convert the Python values to CLR values, as if we just tried As<object>() it would
        // not parse the Python type to a CLR type, only to a new Python type.
        Type[] types = typeof(T).GetGenericArguments();
        object?[] clrValues;

        // If we have more than 8 python values, we are going to have a nested tuple, which we have to unpack.
        if (pyValues.Count() > 8)
        {
            // We are hitting nested tuples here, which will be treated in a different way.
            object?[] firstSeven = pyValues.Take(7).Select((p, i) => p.AsManagedObject(types[i])).ToArray();

            // Get the rest of the values and convert them to a nested tuple.
            IEnumerable<PyObject> rest = pyValues.Skip(7);

            // Back to a Python tuple.
            PyTuple pyTuple = new([.. rest]);

            // Use the decoder pipeline to decode the nested tuple (and its values).
            // We do this because that means if we have nested nested tuples, they'll be decoded as well.
            object? nestedTuple = pyTuple.AsManagedObject(types[7]);

            // Append our nested tuple to the first seven values.
            clrValues = [.. firstSeven, nestedTuple];
        }
        else
        {
            clrValues = pyValues.Select((p, i) => p.AsManagedObject(types[i])).ToArray();
        }

        ConstructorInfo ctor = typeof(T).GetConstructors().First(c => c.GetParameters().Count() == clrValues.Length);
        value = (T)ctor.Invoke(clrValues);

        return true;
    }

    public PyObject TryEncode(object value)
    {
        var t = (ITuple)value;

        List<PyObject> pyObjects = [];

        for (var i = 0; i < t.Length; i++)
        {
            pyObjects.Add(t[i].ToPython());
        }

        return new PyTuple([.. pyObjects]);
    }
}