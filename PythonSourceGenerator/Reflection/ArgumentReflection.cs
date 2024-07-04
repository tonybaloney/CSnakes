using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PythonSourceGenerator.Parser.Types;
using System.Collections.Generic;

namespace PythonSourceGenerator.Reflection;

public class ArgumentReflection
{
    public static ParameterSyntax ArgumentSyntax(string name, string type)
    {
        var reflectedType = TypeReflection.AsPredefinedType(type);
        return SyntaxFactory
            .Parameter(SyntaxFactory.Identifier(name.ToLowerPascalCase()))
            .WithType(reflectedType.Syntax);
            // TODO: Add withdefault
    }

    public static ParameterListSyntax ParameterListSyntax(PythonFunctionParameter[] parameters)
    {
        var parameterListSyntax = new List<ParameterSyntax>();
        foreach (var pythonParameter in parameters)
        {
            var name = pythonParameter.Name;
            var annotation = pythonParameter.Type.ToString(); // TODO: Keep full details
            // TODO : Handle Kind, see https://docs.python.org/3/library/inspect.html#inspect.Parameter
            parameterListSyntax.Add(ArgumentSyntax(name, annotation));
        }
        return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameterListSyntax));
    }
}
