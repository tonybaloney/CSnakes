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
    TypeSyntax ImporterTypeSyntax { get; }

    IEnumerable<StatementSyntax> GenerateCode(string inputName, string outputName);
}

internal static class ResultConversionCodeGenerator
{
    private static readonly IResultConversionCodeGenerator Long = ScalarConversionGenerator(SyntaxKind.LongKeyword, "Int64");
    private static readonly IResultConversionCodeGenerator String = ScalarConversionGenerator(SyntaxKind.StringKeyword, "String");
    private static readonly IResultConversionCodeGenerator Boolean = ScalarConversionGenerator(SyntaxKind.BoolKeyword, "Boolean");
    private static readonly IResultConversionCodeGenerator Double = ScalarConversionGenerator(SyntaxKind.DoubleKeyword, "Double");
    private static readonly IResultConversionCodeGenerator ByteArray = ScalarConversionGenerator(ParseTypeName("byte[]"), "ByteArray");
    private static readonly IResultConversionCodeGenerator Buffer = ScalarConversionGenerator(ParseTypeName("IPyBuffer"), "Buffer");

    public static IEnumerable<StatementSyntax> GenerateCode(PythonTypeSpec pythonTypeSpec,
                                                            string inputName, string outputName) =>
        Create(pythonTypeSpec).GenerateCode(inputName, outputName);

    private static NameSyntax ImportersQualifiedName =>
        ParseName("global::CSnakes.Runtime.Python.Internals.PyObjectImporters");

    public static IResultConversionCodeGenerator Create(PythonTypeSpec pythonTypeSpec)
    {
        switch (pythonTypeSpec)
        {
            case { Name: "int" }: return Long;
            case { Name: "str" }: return String;
            case { Name: "float" }: return Double;
            case { Name: "bool" }: return Boolean;
            case { Name: "bytes" }: return ByteArray;
            case { Name: "Buffer" or "typing.Buffer" or "collections.abc.Buffer" }: return Buffer;

            case { Name: "list" or "typing.List" or "List", Arguments: [var t] }:
            {
                return ListConversionGenerator(t);
            }
            case { Name: "typing.Sequence" or "Sequence", Arguments: [var t] }:
            {
                return ListConversionGenerator(t, "Sequence");
            }
            case { Name: "tuple" or "typing.Tuple" or "Tuple", Arguments: { Length: >= 1 and <= 12 } ts }:
            {
                var generators = ImmutableArray.CreateRange(from t in ts select Create(t));
                return new ConversionGenerator(TupleType(SeparatedList(from item in generators select TupleElement(item.TypeSyntax))),
                                               TypeReflection.CreateGenericType("Tuple", [.. from item in generators select item.TypeSyntax, .. from item in generators select item.ImporterTypeSyntax]));
            }
            case { Name: "dict" or "typing.Dict" or "Dict", Arguments: [var kt, var vt] }:
            {
                return DictionaryConversionGenerator(kt, vt, "Dictionary");
            }
            case { Name: "typing.Mapping" or "Mapping", Arguments: [var kt, var vt] }:
            {
                return DictionaryConversionGenerator(kt, vt, "Mapping");
            }
            case { Name: "typing.Generator" or "Generator", Arguments: [var yt, var st, var rt] }:
            {
                return GeneratorConversionGenerator(yt, st, rt);
            }
            case { Name: "typing.Coroutine" or "Coroutine", Arguments: [var yt, var st, var rt] }:
            {
                return GeneratorConversionGenerator(yt, st, rt, "ICoroutine", "Coroutine");
            }
            case var other:
            {
                var typeSyntax = TypeReflection.AsPredefinedType(other, TypeReflection.ConversionDirection.FromPython);
                return new ConversionGenerator(typeSyntax, TypeReflection.CreateGenericType("Runtime", [typeSyntax]));
            }
        }
    }

    private sealed class ConversionGenerator(TypeSyntax typeSyntax, TypeSyntax importerTypeSyntax) :
        IResultConversionCodeGenerator
    {
        public ConversionGenerator(TypeSyntax typeSyntax, SimpleNameSyntax simpleImporterTypeSyntax) :
            this(typeSyntax, QualifiedName(ImportersQualifiedName, simpleImporterTypeSyntax)) { }

        public TypeSyntax TypeSyntax { get; } = typeSyntax;
        public TypeSyntax ImporterTypeSyntax { get; } = importerTypeSyntax;

        public IEnumerable<StatementSyntax> GenerateCode(string inputName, string outputName) =>
            [ParseStatement($"var {outputName} = {ImporterTypeSyntax}.Import({inputName});")];
    }

    public static IResultConversionCodeGenerator ScalarConversionGenerator(SyntaxKind syntaxKind, string importerTypeName) =>
        ScalarConversionGenerator(PredefinedType(Token(syntaxKind)), importerTypeName);

    public static IResultConversionCodeGenerator ScalarConversionGenerator(TypeSyntax syntax, string importerTypeName) =>
        new ConversionGenerator(syntax, IdentifierName(importerTypeName));

    public static IResultConversionCodeGenerator ListConversionGenerator(PythonTypeSpec itemTypeSpec, string importerTypeName = "List")
    {
        var generator = Create(itemTypeSpec);
        return new ConversionGenerator(TypeReflection.CreateGenericType(nameof(IReadOnlyList<object>), [generator.TypeSyntax]),
                                       TypeReflection.CreateGenericType(importerTypeName, [generator.TypeSyntax, generator.ImporterTypeSyntax]));
    }

    public static IResultConversionCodeGenerator DictionaryConversionGenerator(PythonTypeSpec keyTypeSpec,
                                                                               PythonTypeSpec valueTypeSpec,
                                                                               string importerTypeName)
    {
        var generator = (Key: Create(keyTypeSpec), Value: Create(valueTypeSpec));
        return new ConversionGenerator(TypeReflection.CreateGenericType(nameof(IReadOnlyDictionary<object, object>), [generator.Key.TypeSyntax, generator.Value.TypeSyntax]),
                                       TypeReflection.CreateGenericType(importerTypeName, [generator.Key.TypeSyntax, generator.Value.TypeSyntax, generator.Key.ImporterTypeSyntax, generator.Value.ImporterTypeSyntax]));
    }

    public static IResultConversionCodeGenerator GeneratorConversionGenerator(PythonTypeSpec yieldTypeSpec,
                                                                              PythonTypeSpec sendTypeSpec,
                                                                              PythonTypeSpec returnTypeSpec,
                                                                              string typeName = "IGeneratorIterator",
                                                                              string importerTypeName = "Generator")
    {
        var generator = (Yield: Create(yieldTypeSpec),
                         Send: Create(sendTypeSpec),
                         Return: Create(returnTypeSpec));

        return new ConversionGenerator(TypeReflection.CreateGenericType(typeName, [generator.Yield.TypeSyntax, generator.Send.TypeSyntax, generator.Return.TypeSyntax]),
                                       TypeReflection.CreateGenericType(importerTypeName, [generator.Yield.TypeSyntax, generator.Send.TypeSyntax, generator.Return.TypeSyntax, generator.Yield.ImporterTypeSyntax, generator.Return.ImporterTypeSyntax]));
    }
}
