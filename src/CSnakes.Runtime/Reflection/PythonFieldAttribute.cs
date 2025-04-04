namespace CSnakes.Runtime.Reflection;

[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]

/// <summary>
/// Custom attribute to specify the name of a Python field.
/// <paramref name="name"/> is the name of the field in Python.
/// <paramref name="self"/> indicates if the field is a copy of the self field.
/// </summary>
public sealed class PythonFieldAttribute(string name, bool self = false) : Attribute
{
    public string Name { get; } = name;

    public bool IsSelf { get; } = self;
}
