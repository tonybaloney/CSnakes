using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Python.Runtime;
using System;
using System.Collections.Generic;

namespace PythonSourceGenerator.Reflection;

public static class TypeReflection
{
    public static TypeSyntax AsPredefinedType(string pythonType)
    {
        // If type is an alias, e.g. "list[int]", "list[float]", etc.
        if (pythonType.Contains("[") && pythonType.Contains("]"))
        {
            var genericName = pythonType.Split('[')[0];
            // Get last occurrence of ] in pythonType
            var genericOf = pythonType.Substring(pythonType.IndexOf('[') + 1, pythonType.LastIndexOf(']') - pythonType.IndexOf('[') - 1);
            return genericName switch
            {
                "list" => CreateListType(genericOf),
                "tuple" => CreateTupleType(genericOf),
                "dict" => CreateDictionaryType(genericOf),
                // Todo more types... see https://docs.python.org/3/library/stdtypes.html#standard-generic-classes
                _ => SyntaxFactory.ParseTypeName("PyObject"),// TODO : Should be nullable?
            };
        }
        return pythonType switch
        {
            "int" => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword)),
            "str" => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
            "float" => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword)),
            "bool" => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
            // Todo more types...
            _ => SyntaxFactory.ParseTypeName("PyObject"),// TODO : Should be nullable?
        };
    }

    private static TypeSyntax CreateDictionaryType(string genericOf)
    {
        var types = genericOf.Split(',');

        var genericArgs = new List<TypeSyntax>();
        foreach (var t in types)
        {
            var innerType = AsPredefinedType(t.Trim());
            genericArgs.Add(innerType);
        }

        return CreateGenericType("IReadOnlyDictionary", genericArgs);
    }

    public static string[] SplitTypeArgs(string input)
    {
        if (!input.Contains(","))
        {
            return [input];
        }

        List<string> args = [];
        int cursor = 0;
        while (cursor < input.Length)
        {
            // Get next ,
            var nextComma = input.IndexOf(',', cursor);
            if (nextComma == -1)
            {
                // No more items, stuff last one
                args.Add(input.Substring(cursor).Trim());
                break;
            }
            var nextBracket = input.IndexOf('[', cursor);

            // Is there a [ before the next comma? then it's likely a nested type
            if (nextBracket != -1 && nextBracket < nextComma)
            {
                // Next arg contains [] (and possibly a ,)
                var closingBracket = input.IndexOf(']', cursor);
                if (closingBracket == -1)
                {
                    throw new Exception("Type annotation does not have closing bracket");
                }
                nextComma = input.IndexOf(',', closingBracket);
                if (nextComma == -1)
                {
                    args.Add(input.Substring(cursor).Trim());
                    break;
                }
            }

            // push substring
            args.Add(input.Substring(cursor, nextComma - cursor).Trim());
            cursor = nextComma + 1;
        }

        return [.. args];
    }

    private static TypeSyntax CreateTupleType(string genericOf)
    {
        var tupleTypes = SplitTypeArgs(genericOf);
        var tupleTypeSyntax = new TypeSyntax[tupleTypes.Length];
        if (tupleTypes.Length > 8) // TODO: Implement up to 21
        {
            throw new NotSupportedException("Maximum tuple items is 8");
        }
        for (int i = 0; i < tupleTypes.Length; i++)
        {
            var innerType = AsPredefinedType(tupleTypes[i].Trim());
            tupleTypeSyntax[i] = innerType;
        }
        return CreateGenericType("Tuple", tupleTypeSyntax);
    }

    private static TypeSyntax CreateListType(string genericOf)
    {
        var innerType = AsPredefinedType(genericOf.Trim());
        return CreateGenericType("IEnumerable", [innerType]);
    }

    internal static string AnnotationAsTypeName(PyObject pyObject)
    {
        var typeName = pyObject.GetPythonType().Name;
        // Is class?
        return typeName switch
        {
            "type" or "class" => pyObject.GetAttr("__name__").ToString(),
            _ => pyObject.Repr().ToString(),
        };
    }

    internal static TypeSyntax CreateGenericType(string typeName, IEnumerable<TypeSyntax> genericArguments) =>
        SyntaxFactory.GenericName(
            SyntaxFactory.Identifier(typeName))
            .WithTypeArgumentList(
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SeparatedList(
                        genericArguments)));
}
