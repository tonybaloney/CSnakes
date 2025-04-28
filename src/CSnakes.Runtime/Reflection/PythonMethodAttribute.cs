
namespace CSnakes.Runtime.Reflection;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]

/// <summary>
/// Custom attribute to specify the name of a Python method.
/// <paramref name="name"/> is the name of the method in Python.
/// </summary>
public sealed class PythonMethodAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
