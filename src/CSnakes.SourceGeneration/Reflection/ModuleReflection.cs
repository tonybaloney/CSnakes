using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSnakes.Parser.Types;

namespace CSnakes.Reflection;

public static class ModuleReflection
{
    public static IEnumerable<MethodDefinition> MethodsFromFunctionDefinitions(PythonFunctionDefinition[] functions, string moduleName)
    {
        return functions.Select(function => MethodReflection.FromMethod(function, moduleName));
    }

    public static string Compile(this IEnumerable<MethodDeclarationSyntax> methods)
    {
        using StringWriter sw = new();
        foreach (var method in methods)
        {
            method.NormalizeWhitespace().WriteTo(sw);
        }
        return sw.ToString();
    }
}
