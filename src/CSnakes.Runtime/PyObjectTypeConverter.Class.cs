using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using CSnakes.Runtime.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;


namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    private static object ConvertToClass(PyObject pyObject, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type destinationType)
    {
        // This will work for all custom classes. See https://docs.python.org/3/c-api/typeobj.html#heap-types
        var properties = destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
        var instance = Activator.CreateInstance(destinationType) ?? throw new InvalidOperationException($"Failed to create instance of {destinationType}");
        foreach (var property in properties)
        {
            if (property.Name == "Self")
            {
                property.SetValue(instance, pyObject);
                continue;
            }
            var pythonFieldAttribute = property.GetCustomAttribute<PythonFieldAttribute>();

            if (pythonFieldAttribute?.IsSelf ?? false)
            {
                property.SetValue(instance, pyObject);
                continue;
            }

            string attrName = pythonFieldAttribute?.Name ?? ToSnakeCase(property.Name);
            var value = pyObject.GetAttr(attrName);
            property.SetValue(instance, value.As(property.PropertyType));
        }
        return instance;
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
}
