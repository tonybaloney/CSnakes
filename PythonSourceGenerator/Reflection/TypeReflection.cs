using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PythonSourceGenerator.Reflection;

public class ReflectedType(string convertor, TypeSyntax syntax, IEnumerable<TypeSyntax> genericArguments = null)
{
    public string Convertor => convertor;
    public TypeSyntax Syntax => syntax;
    public IEnumerable<TypeSyntax> GenericArguments => genericArguments;
}

public static class TypeReflection
{
    public static ReflectedType AsPredefinedType(string pythonType)
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
                _ => new(null, SyntaxFactory.ParseTypeName("PyObject")),// TODO : Should be nullable?
            };
        }
        return pythonType switch
        {
            "int" => new("ToInt64", SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword))),
            "str" => new("ToString", SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))),
            "float" => new("ToDouble", SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword))),
            "bool" => new("ToBoolean", SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword))),
            // Todo more types...
            _ => new(null, SyntaxFactory.ParseTypeName("PyObject")),// TODO : Should be nullable?
        };
    }

    private static ReflectedType CreateDictionaryType(string genericOf)
    {
        var types = genericOf.Split(',');

        var genericArgs = new List<TypeSyntax>();
        foreach (var t in types)
        {
            var innerType = AsPredefinedType(t.Trim());
            genericArgs.Add(innerType.Syntax);
        }

        var convertor = $"AsDictionary<{string.Join(", ", genericArgs)}>";
        var syntax = CreateGenericType("IReadOnlyDictionary", genericArgs);

        return new(convertor, syntax, genericArgs);
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

    private static ReflectedType CreateTupleType(string genericOf)
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
            tupleTypeSyntax[i] = innerType.Syntax;
        }
        var convertor = $"AsTuple<{string.Join<TypeSyntax>(", ", tupleTypeSyntax)}>";
        var syntax = CreateGenericType("Tuple", tupleTypeSyntax);

        return new(convertor, syntax, tupleTypeSyntax);
    }

    private static ReflectedType CreateListType(string genericOf)
    {
        var innerType = AsPredefinedType(genericOf.Trim());
        var convertor = $"AsEnumerable<{innerType.Syntax}>";
        var syntax = CreateGenericType("IEnumerable", [innerType.Syntax]);

        return new(convertor, syntax, [innerType.Syntax]);
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
