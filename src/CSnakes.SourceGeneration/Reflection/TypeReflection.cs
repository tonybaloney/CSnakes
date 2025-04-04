using CSnakes.Parser.Types;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace CSnakes.Reflection;

public static class TypeReflection
{
    public enum ConversionDirection
    {
        ToPython,
        FromPython
    }

    public static TypeSyntax AsPredefinedType(PythonTypeSpec pythonType, ConversionDirection direction) =>
        (pythonType, direction) switch
        {
            ({ Name: "list" or "typing.List" or "List" or "typing.Sequence" or "Sequence", Arguments: [var t] }, _) => CreateListType(t, direction),
            ({ Name: "tuple" or "typing.Tuple" or "Tuple", Arguments: var ts }, _) => CreateTupleType(ts, direction),
            ({ Name: "dict" or "typing.Dict" or "Dict" or "typing.Mapping" or "Mapping", Arguments: [var kt, var vt] }, _) => CreateDictionaryType(kt, vt, direction),
            ({ Name: "typing.Optional" or "Optional", Arguments: [var t] }, _) => AsPredefinedType(t, direction),
            ({ Name: "typing.Generator" or "Generator", Arguments: [var yt, var st, var rt] }, _) => CreateGeneratorType(yt, st, rt, direction),
            ({ Name: "typing.Coroutine" or "Coroutine", Arguments: [var yt, var st, var rt] }, _) => CreateCoroutineType(yt, st, rt, direction),
            // Todo more types... see https://docs.python.org/3/library/stdtypes.html#standard-generic-classes
            ({ Name: "int" }, _) => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword)),
            ({ Name: "str" }, _) => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
            ({ Name: "float" }, _) => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword)),
            ({ Name: "bool" }, _) => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
            ({ Name: "bytes" }, _) => SyntaxFactory.ParseTypeName("byte[]"),
            ({ Name: "Buffer" or "typing.Buffer" or "collections.abc.Buffer" }, ConversionDirection.FromPython) => SyntaxFactory.ParseTypeName("IPyBuffer"),
            _ => SyntaxFactory.ParseTypeName("PyObject"),
        };

    private static TypeSyntax CreateDictionaryType(PythonTypeSpec keyType, PythonTypeSpec valueType, ConversionDirection direction) => CreateGenericType("IReadOnlyDictionary", [
            AsPredefinedType(keyType, direction),
            AsPredefinedType(valueType, direction)
            ]);

    private static TypeSyntax CreateGeneratorType(PythonTypeSpec yieldType, PythonTypeSpec sendType, PythonTypeSpec returnType, ConversionDirection direction) => CreateGenericType("IGeneratorIterator", [
            AsPredefinedType(yieldType, direction),
            AsPredefinedType(sendType, direction),
            AsPredefinedType(returnType, direction)
            ]);

    private static TypeSyntax CreateCoroutineType(PythonTypeSpec yieldType, PythonTypeSpec sendType, PythonTypeSpec returnType, ConversionDirection direction) => CreateGenericType("ICoroutine", [
            AsPredefinedType(yieldType, direction),
            AsPredefinedType(sendType, direction),
            AsPredefinedType(returnType, direction)
            ]);

    private static TypeSyntax CreateTupleType(ImmutableArray<PythonTypeSpec> tupleTypes, ConversionDirection direction)
    {
        if (tupleTypes.Length == 1)
        {
            return CreateGenericType("ValueTuple", tupleTypes.Select(t => AsPredefinedType(t, direction)));
        }

        IEnumerable<TupleElementSyntax> tupleTypeSyntaxGroups = tupleTypes.Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / 7)
            .Select(x => x.Select(v => v.Value))
            .Select(typeSpecs => typeSpecs.Select(t => AsPredefinedType(t, direction)))
            .SelectMany(item => item.Select(SyntaxFactory.TupleElement));

        return SyntaxFactory.TupleType(
            SyntaxFactory.Token(SyntaxKind.OpenParenToken),
            SyntaxFactory.SeparatedList(tupleTypeSyntaxGroups),
            SyntaxFactory.Token(SyntaxKind.CloseParenToken));
    }

    private static TypeSyntax CreateListType(PythonTypeSpec genericOf, ConversionDirection direction) => CreateGenericType("IReadOnlyList", [AsPredefinedType(genericOf, direction)]);

    internal static TypeSyntax CreateGenericType(string typeName, IEnumerable<TypeSyntax> genericArguments) =>
        SyntaxFactory.GenericName(
            SyntaxFactory.Identifier(typeName))
            .WithTypeArgumentList(
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SeparatedList(
                        genericArguments)));
}
