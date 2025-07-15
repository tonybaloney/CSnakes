using CSnakes.Parser.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSnakes.Reflection;

public static class ModuleReflection
{
    public static IEnumerable<MethodDefinition> MethodsFromFunctionDefinitions(IEnumerable<PythonFunctionDefinition> functions, string moduleName)
    {
        return functions.SelectMany(function => MethodReflection.FromMethod(function, moduleName))
                        .Distinct(new MethodDefinitionComparator());
    }

    public static string Compile(this IEnumerable<MethodDeclarationSyntax> methods)
    {
        using StringWriter sw = new();
        foreach (var (i, method) in methods.Select((m, i) => (i, m)))
        {
            if (i > 0)
                sw.WriteLine();
            method.NormalizeWhitespace().WriteTo(sw);
            sw.WriteLine();
        }
        return sw.ToString();
    }
}
