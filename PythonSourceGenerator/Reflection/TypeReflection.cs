using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PythonSourceGenerator.Parser.Types;
using System;
using System.Collections.Generic;

namespace PythonSourceGenerator.Reflection;

public static class TypeReflection
{
    public static TypeSyntax AsPredefinedType(PythonTypeSpec pythonType)
    {
        // If type is an alias, e.g. "list[int]", "list[float]", etc.
        if (pythonType.HasArguments())
        {
            var genericName = pythonType.Name;
            // Get last occurrence of ] in pythonType
            return genericName switch
            {
                "list" => CreateListType(pythonType.Arguments[0]),
                "tuple" => CreateTupleType(pythonType.Arguments),
                "dict" => CreateDictionaryType(pythonType.Arguments[0], pythonType.Arguments[1]),
                // Todo more types... see https://docs.python.org/3/library/stdtypes.html#standard-generic-classes
                _ => SyntaxFactory.ParseTypeName("PyObject"),// TODO : Should be nullable?
            };
        }
        return pythonType.Name switch
        {
            "int" => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword)),
            "str" => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
            "float" => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword)),
            "bool" => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
            // Todo more types...
            _ => SyntaxFactory.ParseTypeName("PyObject"),// TODO : Should be nullable?
        };
    }

    private static TypeSyntax CreateDictionaryType(PythonTypeSpec keyType, PythonTypeSpec valueType) => CreateGenericType("IReadOnlyDictionary", [
            AsPredefinedType(keyType),
            AsPredefinedType(valueType)
            ]);

    private static TypeSyntax CreateTupleType(PythonTypeSpec[] tupleTypes)
    {
        var tupleTypeSyntax = new TypeSyntax[tupleTypes.Length];
        if (tupleTypes.Length > 8) // TODO: (track) Implement up to 21
        {
            throw new NotSupportedException("Maximum tuple items is 8");
        }
        for (int i = 0; i < tupleTypes.Length; i++)
        {
            tupleTypeSyntax[i] = AsPredefinedType(tupleTypes[i]);
        }
        return CreateGenericType("Tuple", tupleTypeSyntax);
    }

    private static TypeSyntax CreateListType(PythonTypeSpec genericOf) => CreateGenericType("IEnumerable", [AsPredefinedType(genericOf)]);

    internal static TypeSyntax CreateGenericType(string typeName, IEnumerable<TypeSyntax> genericArguments) =>
        SyntaxFactory.GenericName(
            SyntaxFactory.Identifier(typeName))
            .WithTypeArgumentList(
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SeparatedList(
                        genericArguments)));
}
