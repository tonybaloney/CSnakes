using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CSnakes.Runtime.Convertors
{
    public class TupleConvertor : IPythonConvertor
    {
        public bool CanDecode(PyObject objectType, Type targetType) => typeof(ITuple).IsAssignableFrom(targetType);

        public bool CanEncode(Type type) => typeof(ITuple).IsAssignableFrom(type);

        public bool TryDecode(PyObject pyObj, out object? value)
        {
            var tuplePtr = pyObj.DangerousGetHandle();
            if (!CPythonAPI.IsPyTuple(tuplePtr))
            {
                value = null;
                // TODO: Set the reason as a type error.
                return false;
            }

            // We have to convert the Python values to CLR values, as if we just tried As<object>() it would
            // not parse the Python type to a CLR type, only to a new Python type.
            Type[] types = typeof(ITuple).GetGenericArguments();
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
                object?[] firstSeven = tupleValues.Take(7).Select((p, i) => p.AsManagedObject(types[i])).ToArray();

                // Get the rest of the values and convert them to a nested tuple.
                IEnumerable<PyObject> rest = tupleValues.Skip(7);

                // Back to a Python tuple.
                PyObject pyTuple = PyTuple.CreateTuple(rest);

                // Use the decoder pipeline to decode the nested tuple (and its values).
                // We do this because that means if we have nested nested tuples, they'll be decoded as well.
                object? nestedTuple = pyTuple.AsManagedObject(types[7]);

                // Append our nested tuple to the first seven values.
                clrValues = [.. firstSeven, nestedTuple];
            }
            else
            {
                clrValues = tupleValues.Select((p, i) => p.AsManagedObject(types[i])).ToArray();
            }

            ConstructorInfo ctor = typeof(ITuple).GetConstructors().First(c => c.GetParameters().Count() == clrValues.Length);
            value = (ITuple)ctor.Invoke(clrValues);

            return true;
        }

        public bool TryEncode(object value, out PyObject? result)
        {
            var t = (ITuple)value;

            List<PyObject> pyObjects = [];

            for (var i = 0; i < t.Length; i++)
            {
                pyObjects.Add(t[i].ToPython());
            }

            result = PyTuple.CreateTuple(pyObjects);
            return true;
        }
    }
}