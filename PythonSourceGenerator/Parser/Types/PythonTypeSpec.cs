using System.Linq;

namespace PythonSourceGenerator.Parser.Types;
public class PythonTypeSpec
{
    public string Name { get; set; }

    public PythonTypeSpec[] Arguments { get; set; }

    public override string ToString()
    {
        if (Arguments != null && Arguments.Length > 0)
        {
            return $"{Name}[{string.Join(", ", Arguments.Select(a => a.ToString()))}]";
        }
        return Name;
    }

    public bool HasArguments()
    {
        return Arguments != null && Arguments.Length > 0;
    }
}
