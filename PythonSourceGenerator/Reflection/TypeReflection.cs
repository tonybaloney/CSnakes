using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PythonSourceGenerator.Parser.Types;
using System;
using System.Collections.Generic;

namespace PythonSourceGenerator.Reflection;

public class ReflectedType(string convertor, TypeSyntax syntax, IEnumerable<TypeSyntax> genericArguments = null)
{
    public string Convertor => convertor;
    public TypeSyntax Syntax => syntax;
    public IEnumerable<TypeSyntax> GenericArguments => genericArguments;
}

public static class TypeReflection
{
    public static ReflectedType AsPredefinedType(PythonTypeSpec pythonType)
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
                _ => new(null, SyntaxFactory.ParseTypeName("PyObject")),// TODO : Should be nullable?
            };
        }
        return pythonType.Name switch
        {
            "int" => new("ToInt64", SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword))),
            "str" => new("ToString", SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))),
            "float" => new("ToDouble", SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword))),
            "bool" => new("ToBoolean", SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword))),
            // Todo more types...
            _ => new(null, SyntaxFactory.ParseTypeName("PyObject")),// TODO : Should be nullable?
        };
    }

    private static ReflectedType CreateDictionaryType(PythonTypeSpec keyType, PythonTypeSpec valueType)
    {
        TypeSyntax[] genericArgs = [
            AsPredefinedType(keyType).Syntax,
            AsPredefinedType(valueType).Syntax
            ];

        var convertor = $"AsDictionary<{genericArgs[0]}, {genericArgs[1]}>";
        var syntax = CreateGenericType("IReadOnlyDictionary", genericArgs);

        return new(convertor, syntax, genericArgs);
    }

    private static ReflectedType CreateTupleType(PythonTypeSpec[] tupleTypes)
    {
        var tupleTypeSyntax = new TypeSyntax[tupleTypes.Length];
        if (tupleTypes.Length > 8) // TODO: Implement up to 21
        {
            throw new NotSupportedException("Maximum tuple items is 8");
        }
        for (int i = 0; i < tupleTypes.Length; i++)
        {
            var innerType = AsPredefinedType(tupleTypes[i]);
            tupleTypeSyntax[i] = innerType.Syntax;
        }
        var convertor = $"AsTuple<{string.Join<TypeSyntax>(", ", tupleTypeSyntax)}>";
        var syntax = CreateGenericType("Tuple", tupleTypeSyntax);

        return new(convertor, syntax, tupleTypeSyntax);
    }

    private static ReflectedType CreateListType(PythonTypeSpec genericOf)
    {
        var innerType = AsPredefinedType(genericOf);
        var convertor = $"AsEnumerable<{innerType.Syntax}>";
        var syntax = CreateGenericType("IEnumerable", [innerType.Syntax]);

        return new(convertor, syntax, [innerType.Syntax]);
    }

    internal static TypeSyntax CreateGenericType(string typeName, IEnumerable<TypeSyntax> genericArguments) =>
        SyntaxFactory.GenericName(
            SyntaxFactory.Identifier(typeName))
            .WithTypeArgumentList(
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SeparatedList(
                        genericArguments)));
}
