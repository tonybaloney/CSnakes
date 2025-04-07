using CSnakes.Parser.Types;
using CSnakes.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSnakes.SourceGeneration;

internal interface IResultConversionCodeGenerator
{
    TypeSyntax TypeSyntax { get; }
    TypeSyntax ConverterTypeSyntax { get; }

    IEnumerable<StatementSyntax> GenerateCode(string inputName, string outputName);
}

internal static class ResultConversionCodeGenerator
{
    private static readonly IResultConversionCodeGenerator Long = ScalarConversionGenerator(SyntaxKind.LongKeyword, "Int64");
    private static readonly IResultConversionCodeGenerator String = ScalarConversionGenerator(SyntaxKind.StringKeyword, "String");
    private static readonly IResultConversionCodeGenerator Boolean = ScalarConversionGenerator(SyntaxKind.BoolKeyword, "Boolean");
    private static readonly IResultConversionCodeGenerator Double = ScalarConversionGenerator(SyntaxKind.DoubleKeyword, "Double");
    private static readonly IResultConversionCodeGenerator ByteArray = ScalarConversionGenerator(ParseTypeName("byte[]"), "ByteArray");

    public static IEnumerable<StatementSyntax> GenerateCode(PythonTypeSpec pythonTypeSpec,
                                                            string inputName, string outputName) =>
        Create(pythonTypeSpec).GenerateCode(inputName, outputName);

    private static NameSyntax ConvertersQualifiedName =>
        ParseName("global::CSnakes.Runtime.Python.InternalServices.Converters");

    public static IResultConversionCodeGenerator Create(PythonTypeSpec pythonTypeSpec)
    {
        switch (pythonTypeSpec)
        {
            case { Name: "int" }: return Long;
            case { Name: "str" }: return String;
            case { Name: "float" }: return Double;
            case { Name: "bool" }: return Boolean;
            case { Name: "bytes" }: return ByteArray;

            case { Name: "list" or "typing.List" or "List", Arguments: [var t] }:
            {
                return ListConversionGenerator(t, "List");
            }
            case { Name: "typing.Sequence" or "Sequence", Arguments: [var t] }:
            {
                return ListConversionGenerator(t, "Sequence");
            }
            case { Name: "tuple" or "typing.Tuple" or "Tuple", Arguments: { Length: >= 1 and <= 12 } ts }:
            {
                var generators = ImmutableArray.CreateRange(from t in ts select Create(t));
                return new ConversionGenerator(TupleType(SeparatedList(from item in generators select TupleElement(item.TypeSyntax))),
                                               GenericName(Identifier("Tuple"), TypeArgumentList(SeparatedList([.. from item in generators select item.TypeSyntax, .. from item in generators select item.ConverterTypeSyntax]))));
            }
            case { Name: "typing.Coroutine" or "Coroutine", Arguments: [var yt, var st, var rt] }:
            {
                var generator = (Yield: Create(yt), Send: Create(st), Return: Create(rt));
                return new ConversionGenerator(TypeReflection.CreateGenericType("ICoroutine", [generator.Yield.TypeSyntax, generator.Send.TypeSyntax, generator.Return.TypeSyntax]),
                                               GenericName(Identifier("Coroutine"), TypeArgumentList(SeparatedList([generator.Yield.TypeSyntax, generator.Send.TypeSyntax, generator.Return.TypeSyntax, generator.Yield.ConverterTypeSyntax, generator.Send.ConverterTypeSyntax, generator.Return.ConverterTypeSyntax]))));
            }
            case var other:
            {
                var typeSyntax = TypeReflection.AsPredefinedType(other, TypeReflection.ConversionDirection.FromPython);
                return new ConversionGenerator(typeSyntax, GenericName(Identifier("Runtime"), TypeArgumentList(SingletonSeparatedList(typeSyntax))));
            }
        }
    }

    private sealed class ConversionGenerator(TypeSyntax typeSyntax, TypeSyntax converterTypeSyntax) :
        IResultConversionCodeGenerator
    {
        public ConversionGenerator(TypeSyntax typeSyntax, SimpleNameSyntax simpleConverterTypeNameSyntax) :
            this(typeSyntax, QualifiedName(ConvertersQualifiedName, simpleConverterTypeNameSyntax)) { }

        public TypeSyntax TypeSyntax { get; } = typeSyntax;
        public TypeSyntax ConverterTypeSyntax { get; } = converterTypeSyntax;

        public IEnumerable<StatementSyntax> GenerateCode(string inputName, string outputName) =>
            [ParseStatement($"var {outputName} = {ConverterTypeSyntax}.Convert({inputName});")];
    }

    public static IResultConversionCodeGenerator ScalarConversionGenerator(SyntaxKind syntaxKind, string converterTypeName) =>
        ScalarConversionGenerator(PredefinedType(Token(syntaxKind)), converterTypeName);

    public static IResultConversionCodeGenerator ScalarConversionGenerator(TypeSyntax syntax, string converterTypeName) =>
        new ConversionGenerator(syntax, IdentifierName(converterTypeName));

    public static IResultConversionCodeGenerator ListConversionGenerator(PythonTypeSpec itemTypeSpec, string converterTypeName)
    {
        var generator = Create(itemTypeSpec);
        return new ConversionGenerator(GenericName(Identifier(nameof(IReadOnlyList<object>)), TypeArgumentList(SingletonSeparatedList(generator.TypeSyntax))),
                                       GenericName(Identifier(converterTypeName), TypeArgumentList(SeparatedList([generator.TypeSyntax, generator.ConverterTypeSyntax]))));
    }
}
