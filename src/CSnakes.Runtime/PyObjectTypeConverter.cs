using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    private static readonly ConcurrentDictionary<Type, DynamicTypeInfo> knownDynamicTypes = [];

    public static object PyObjectToManagedType(PyObject pyObject, Type destinationType)
    {
        if (CPythonAPI.IsPyDict(pyObject) && IsAssignableToGenericType(destinationType, dictionaryType))
        {
            return ConvertToDictionary(pyObject, destinationType);
        }

        if (CPythonAPI.IsPyList(pyObject) && IsAssignableToGenericType(destinationType, listType))
        {
            return ConvertToList(pyObject, destinationType);
        }

        // This needs to come after lists, because sequences are also maps
        if (CPythonAPI.IsPyMappingWithItems(pyObject) && destinationType.IsAssignableTo(collectionType))
        {
            return ConvertToDictionary(pyObject, destinationType, useMappingProtocol: true);
        }

        if (CPythonAPI.IsPySequence(pyObject) && IsAssignableToGenericType(destinationType, listType))
        {
            return ConvertToList(pyObject, destinationType);
        }

        if (CPythonAPI.IsBuffer(pyObject) && destinationType.IsAssignableTo(bufferType))
        {
            return new PyBuffer(pyObject);
        }

        // Need to find a better way to detect that a PyObject is a class instance
        if (pyObject.GetPythonType().ToString().StartsWith("<class"))
        {
            var properties = destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
            var instance = Activator.CreateInstance(destinationType) ?? throw new InvalidOperationException($"Failed to create instance of {destinationType}");
            foreach (var property in properties)
            {
                var pythonNameAttribute = property.GetCustomAttribute<PythonNameAttribute>();
                string attrName = pythonNameAttribute?.Name ?? ToSnakeCase(property.Name);
                var value = pyObject.GetAttr(attrName);
                property.SetValue(instance, value.As(property.PropertyType));
            }
            return instance;
        }

        throw new InvalidCastException($"Attempting to cast {destinationType} from {pyObject.GetPythonType()}");
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var stringBuilder = new StringBuilder();
        for (int i = 0; i < value.Length; i++)
        {
            if (char.IsUpper(value[i]))
            {
                if (i > 0)
                {
                    stringBuilder.Append('_');
                }
                stringBuilder.Append(char.ToLower(value[i]));
            }
            else
            {
                stringBuilder.Append(value[i]);
            }
        }

        return stringBuilder.ToString();
    }

    record DynamicTypeInfo(ConstructorInfo ReturnTypeConstructor, ConstructorInfo? TransientTypeConstructor = null);
}
