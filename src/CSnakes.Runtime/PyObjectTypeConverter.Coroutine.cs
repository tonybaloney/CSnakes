using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    [RequiresDynamicCode(DynamicCodeMessages.CallsMakeGenericType)]
    internal static object ConvertToCoroutine(PyObject pyObject, Type destinationType)
    {
        Debug.Assert(destinationType.IsGenericType);
        Debug.Assert(!destinationType.IsGenericTypeDefinition);
        Debug.Assert(destinationType.GetGenericTypeDefinition() == typeof(ICoroutine<,,>));

        if (!CPythonAPI.IsPyCoroutine(pyObject))
        {
            throw new InvalidCastException($"Cannot convert {pyObject.GetPythonType()} to a coroutine.");
        }

        if (!knownDynamicTypes.TryGetValue(destinationType, out DynamicTypeInfo? typeInfo))
        {
            var typeArgs = destinationType.GetGenericArguments();
            Type coroutineType = typeof(Coroutine<,,>).MakeGenericType(typeArgs);
            ConstructorInfo ctor = coroutineType.GetConstructors().First();
            typeInfo = new(ctor);
            knownDynamicTypes[destinationType] = typeInfo;
        }

        return typeInfo.ReturnTypeConstructor.Invoke([pyObject.Clone()]);
    }
}
