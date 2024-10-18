namespace CSnakes.Runtime;
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class PythonNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
