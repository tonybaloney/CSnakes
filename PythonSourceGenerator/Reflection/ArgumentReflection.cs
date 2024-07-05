using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PythonSourceGenerator.Parser.Types;
using System.Collections.Generic;

namespace PythonSourceGenerator.Reflection;

public class ArgumentReflection
{
    public static ParameterSyntax ArgumentSyntax(PythonFunctionParameter parameter)
    {
        var reflectedType = TypeReflection.AsPredefinedType(parameter.Type);
        return SyntaxFactory
            .Parameter(SyntaxFactory.Identifier(parameter.Name.ToLowerPascalCase()))
            .WithType(reflectedType.Syntax);
            // TODO: Add withdefault
    }

    public static ParameterListSyntax ParameterListSyntax(PythonFunctionParameter[] parameters)
    {
        var parameterListSyntax = new List<ParameterSyntax>();
        foreach (var pythonParameter in parameters)
        {
            // TODO : Handle Kind, see https://docs.python.org/3/library/inspect.html#inspect.Parameter
            parameterListSyntax.Add(ArgumentSyntax(pythonParameter));
        }
        return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameterListSyntax));
    }
}
