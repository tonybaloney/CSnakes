using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PythonSourceGenerator.Parser.Types;
using System.Collections.Generic;
using System.IO;

namespace PythonSourceGenerator.Reflection;

public static class ModuleReflection
{
    public static List<MethodDefinition> MethodsFromFunctionDefinitions(PythonFunctionDefinition[] functions, string moduleName)
    {
        var methods = new List<MethodDefinition>();
        // Get methods
        foreach (var function in functions)
        {
            methods.Add(MethodReflection.FromMethod(function, moduleName.ToString()));
        }
        return methods;
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
