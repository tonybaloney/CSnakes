using CSnakes.Runtime.Python;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    private object? ConvertToGeneratorIterator(PyObject pyObject, Type destinationType, ITypeDescriptorContext? context, CultureInfo? culture)
    {
        if (!knownDynamicTypes.TryGetValue(destinationType, out DynamicTypeInfo? typeInfo))
        {
            Type item1Type = destinationType.GetGenericArguments()[0];
            Type item2Type = destinationType.GetGenericArguments()[1];
            Type item3Type = destinationType.GetGenericArguments()[2];
            Type generatorType = typeof(GeneratorIterator<,,>).MakeGenericType(item1Type, item2Type, item3Type);
            ConstructorInfo ctor = generatorType.GetConstructors().First();
            typeInfo = new(ctor);
            knownDynamicTypes[destinationType] = typeInfo;
        }

        return typeInfo.ReturnTypeConstructor.Invoke([pyObject.Clone()]);
    }
}