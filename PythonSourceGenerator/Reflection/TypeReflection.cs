using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Python.Runtime;
using System;

namespace PythonSourceGenerator.Reflection;

public static class TypeReflection
{
    public static (string convertor, TypeSyntax syntax) AsPredefinedType(string pythonType)
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
                // Todo more types... see https://docs.python.org/3/library/stdtypes.html#standard-generic-classes
                _ => (null, SyntaxFactory.ParseTypeName("PyObject")),// TODO : Should be nullable?
            };
        }
        return pythonType switch
        {
            "int" => ((string convertor, TypeSyntax syntax))("ToInt64", SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword))),
            "str" => ((string convertor, TypeSyntax syntax))("ToString", SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))),
            "float" => ((string convertor, TypeSyntax syntax))("ToDouble", SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword))),
            "bool" => ((string convertor, TypeSyntax syntax))("ToBoolean", SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword))),
            // Todo more types...
            _ => (null, SyntaxFactory.ParseTypeName("PyObject")),// TODO : Should be nullable?
        };

        static (string convertor, TypeSyntax syntax) CreateListType(string genericOf)
        {
            var (_, innerTypeSyntax) = AsPredefinedType(genericOf.Trim());
            var convertor = "AsEnumerable<" + innerTypeSyntax.ToString() + ">";
            return (convertor, SyntaxFactory.GenericName(
                SyntaxFactory.Identifier("IEnumerable"))
                .WithTypeArgumentList(
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                            innerTypeSyntax))));
        }

        static (string convertor, TypeSyntax syntax) CreateTupleType(string genericOf)
        {
            var tupleTypes = genericOf.Split(',');
            var tupleTypeSyntax = new TypeSyntax[tupleTypes.Length];
            if (tupleTypes.Length > 8) // TODO: Implement up to 21
            {
                throw new NotSupportedException("Maximum tuple items is 8");
            }
            for (int i = 0; i < tupleTypes.Length; i++)
            {
                var (_, innerTypeSyntax) = AsPredefinedType(tupleTypes[i].Trim());
                tupleTypeSyntax[i] = innerTypeSyntax;
            }
            var convertor = "AsTuple<" + string.Join<TypeSyntax>(", ", tupleTypeSyntax) + ">";
            return (convertor, SyntaxFactory.GenericName(
                SyntaxFactory.Identifier("Tuple"))
                .WithTypeArgumentList(
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SeparatedList<TypeSyntax>(
                            tupleTypeSyntax))));
        }
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
}
