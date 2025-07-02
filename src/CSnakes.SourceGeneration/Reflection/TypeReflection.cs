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

    public static IEnumerable<TypeSyntax> AsPredefinedType(PythonTypeSpec pythonType, ConversionDirection direction) =>
     (pythonType, direction) switch
        {
            ({ Name: "list" or "typing.List" or "List" or "typing.Sequence" or "Sequence", Arguments: [var t] }, _) => CreateListType(t, direction),
            ({ Name: "tuple" or "typing.Tuple" or "Tuple", Arguments: var ts }, _) => CreateTupleType(ts, direction),
            ({ Name: "dict" or "typing.Dict" or "Dict" or "typing.Mapping" or "Mapping", Arguments: [var kt, var vt] }, _) => CreateDictionaryType(kt, vt, direction),
            ({ Name: "typing.Optional" or "Optional", Arguments: [var t] }, _) => AsPredefinedType(t, direction).Select(SyntaxFactory.NullableType),
            ({ Name: "typing.Generator" or "Generator", Arguments: [var yt, var st, var rt] }, _) => CreateGeneratorType(yt, st, rt, direction),
            ({ Name: "typing.Coroutine" or "Coroutine", Arguments: [var yt, var st, var rt] }, _) => CreateCoroutineType(yt, st, rt, direction),
            ({ Name: "typing.Union" or "Union", Arguments: var ts }, ConversionDirection.ToPython) => [.. ts.SelectMany(t => AsPredefinedType(t, direction))],
            // Todo more types... see https://docs.python.org/3/library/stdtypes.html#standard-generic-classes
            ({ Name: "int" }, _) => [SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword))],
            ({ Name: "str" }, _) => [SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))],
            ({ Name: "float" }, _) => [SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword))],
            ({ Name: "bool" }, _) => [SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword))],
            ({ Name: "bytes" }, _) => [SyntaxFactory.ParseTypeName("byte[]")],
            ({ Name: "Buffer" or "collections.abc.Buffer" }, ConversionDirection.FromPython) => [SyntaxFactory.ParseTypeName("IPyBuffer")],
            _ => [SyntaxFactory.ParseTypeName("PyObject")],
        };

    private static IEnumerable<TypeSyntax> CreateDictionaryType(PythonTypeSpec keyType, PythonTypeSpec valueType, ConversionDirection direction)
    {
        return from type in AsPredefinedType(keyType, direction)
               from value in AsPredefinedType(valueType, direction)
               select CreateGenericType("IReadOnlyDictionary", [type, value]);
    }

    private static IEnumerable<TypeSyntax> CreateGeneratorType(PythonTypeSpec yieldType, PythonTypeSpec sendType, PythonTypeSpec returnType, ConversionDirection direction) {
        return from yieldTypeI in AsPredefinedType(yieldType, direction)
               from sendTypeI in AsPredefinedType(sendType, direction)
               from returnTypeI in AsPredefinedType(returnType, direction)
               select CreateGenericType("IGeneratorIterator", [yieldTypeI, sendTypeI, returnTypeI]);
    }

    private static IEnumerable<TypeSyntax> CreateCoroutineType(PythonTypeSpec yieldType, PythonTypeSpec sendType, PythonTypeSpec returnType, ConversionDirection direction)
    {
        return from yieldTypeI in AsPredefinedType(yieldType, direction)
               from sendTypeI in AsPredefinedType(sendType, direction)
               from returnTypeI in AsPredefinedType(returnType, direction)
               select CreateGenericType("ICoroutine", [yieldTypeI, sendTypeI, returnTypeI]);
    }

    private static IEnumerable<TypeSyntax> CreateTupleType(ImmutableArray<PythonTypeSpec> tupleTypes, ConversionDirection direction)
    {
        if (tupleTypes.Length == 1)
        {
            foreach (var type in AsPredefinedType(tupleTypes[0], direction))
                yield return CreateGenericType("ValueTuple", [type]);
            yield break;
        }

        var tupleTypeSpecs = tupleTypes.Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / 7)
            .Select(x => x.Select(v => v.Value))
            /* TODO: Iterate potential Tuple types */
            .Select(typeSpecs => typeSpecs.Select(t => AsPredefinedType(t, direction).First()))
            .SelectMany(item => item.Select(SyntaxFactory.TupleElement));

        yield return SyntaxFactory.TupleType(
            SyntaxFactory.Token(SyntaxKind.OpenParenToken),
            SyntaxFactory.SeparatedList(tupleTypeSpecs),
            SyntaxFactory.Token(SyntaxKind.CloseParenToken));
    }

    private static IEnumerable<TypeSyntax> CreateListType(PythonTypeSpec genericOf, ConversionDirection direction)
    {
        return from listType in AsPredefinedType(genericOf, direction)
               select CreateGenericType("IReadOnlyList", [listType]);
    }

    internal static GenericNameSyntax CreateGenericType(string typeName, IEnumerable<TypeSyntax> genericArguments) =>
        SyntaxFactory.GenericName(
            SyntaxFactory.Identifier(typeName))
            .WithTypeArgumentList(
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SeparatedList(
                        genericArguments)));
}
