namespace CSnakes.Runtime.Reflection;

[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]

/// <summary>
/// Custom attribute to specify the name of a Python field.
/// <paramref name="name"/> is the name of the field in Python.
/// </summary>
public sealed class PythonFieldAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
