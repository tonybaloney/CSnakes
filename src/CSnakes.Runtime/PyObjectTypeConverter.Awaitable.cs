using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    [RequiresDynamicCode(DynamicCodeMessages.CallsMakeGenericType)]
    internal static object ConvertToAwaitable(PyObject pyObject, Type destinationType)
    {
        Debug.Assert(destinationType.IsGenericType);
        Debug.Assert(!destinationType.IsGenericTypeDefinition);
        Debug.Assert(destinationType.GetGenericTypeDefinition() == typeof(IAwaitable<>));

        if (!CPythonAPI.IsPyAwaitable(pyObject))
        {
            throw new InvalidCastException($"Cannot convert {pyObject.GetPythonType()} to an awaitable.");
        }

        if (!knownDynamicTypes.TryGetValue(destinationType, out DynamicTypeInfo? typeInfo))
        {
            var typeArgs = destinationType.GetGenericArguments();
            Type awaitableType = typeof(Awaitable<>).MakeGenericType(typeArgs);
            ConstructorInfo ctor = awaitableType.GetConstructors().First();
            typeInfo = new(ctor);
            knownDynamicTypes[destinationType] = typeInfo;
        }

        return typeInfo.ReturnTypeConstructor.Invoke([pyObject.Clone()]);
    }
}
