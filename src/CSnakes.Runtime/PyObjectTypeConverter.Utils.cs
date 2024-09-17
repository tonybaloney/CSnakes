using System.Collections.Concurrent;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    private static readonly ConcurrentDictionary<(Type, Type), bool> assignableGenericsMap = [];

    public static bool IsAssignableToGenericType(Type givenType, Type genericType)
    {
        if (assignableGenericsMap.TryGetValue((givenType, genericType), out bool result))
            return result;
        var interfaceTypes = givenType.GetInterfaces();

        foreach (var it in interfaceTypes)
        {
            if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
            {
                assignableGenericsMap[(givenType, genericType)] = true;
                return true;
            }
        }

        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
        {
            assignableGenericsMap[(givenType, genericType)] = true;
            return true;
        }

        Type? baseType = givenType.BaseType;
        if (baseType is null)
        {
            assignableGenericsMap[(givenType, genericType)] = false;
            return false;
        }

        return IsAssignableToGenericType(baseType, genericType);
    }
}
