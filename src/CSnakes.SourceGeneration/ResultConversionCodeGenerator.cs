using CSnakes.Parser.Types;
using CSnakes.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using TupleType = CSnakes.Parser.Types.TupleType;

namespace CSnakes.SourceGeneration;

internal interface IResultConversionCodeGenerator
{
    TypeSyntax TypeSyntax { get; }
    TypeSyntax ImporterTypeSyntax { get; }

    IEnumerable<StatementSyntax> GenerateCode(string inputName, string outputName,
                                              string cancellationTokenName);
}

internal static class ResultConversionCodeGenerator
{
    private static readonly IResultConversionCodeGenerator None = ScalarConversionGenerator(ParseTypeName("PyObject"), "None");
    private static readonly IResultConversionCodeGenerator Any = ScalarConversionGenerator(ParseTypeName("PyObject"), "Clone");
    private static readonly IResultConversionCodeGenerator Long = ScalarConversionGenerator(SyntaxKind.LongKeyword, "Int64");
    private static readonly IResultConversionCodeGenerator String = ScalarConversionGenerator(SyntaxKind.StringKeyword, "String");
    private static readonly IResultConversionCodeGenerator Boolean = ScalarConversionGenerator(SyntaxKind.BoolKeyword, "Boolean");
    private static readonly IResultConversionCodeGenerator Double = ScalarConversionGenerator(SyntaxKind.DoubleKeyword, "Double");
    private static readonly IResultConversionCodeGenerator ByteArray = ScalarConversionGenerator(ParseTypeName("byte[]"), "ByteArray");
    private static readonly IResultConversionCodeGenerator Buffer = ScalarConversionGenerator(ParseTypeName("IPyBuffer"), "Buffer");

    public static IEnumerable<StatementSyntax> GenerateCode(PythonTypeSpec pythonTypeSpec,
                                                            string inputName, string outputName,
                                                            string cancellationTokenName) =>
        Create(pythonTypeSpec).GenerateCode(inputName, outputName, cancellationTokenName);

    private static NameSyntax ImportersQualifiedName =>
        ParseName("global::CSnakes.Runtime.Python.PyObjectImporters");

    public static IResultConversionCodeGenerator Create(PythonTypeSpec pythonTypeSpec)
    {
        switch (pythonTypeSpec)
        {
            case NoneType: return None;
            case AnyType: return Any;
            case IntType: return Long;
            case StrType: return String;
            case FloatType: return Double;
            case BoolType: return Boolean;
            case BytesType: return ByteArray;
            case BufferType: return Buffer;

            case OptionalType { Of: var t and (IntType or FloatType or BoolType or TupleType) }:
            {
                return OptionalConversionGenerator(t, "OptionalValue");
            }
            case OptionalType { Of: var t }:
            {
                return OptionalConversionGenerator(t);
            }
            case ListType { Of: var t }:
            {
                return ListConversionGenerator(t);
            }
            case SequenceType { Of: var t }:
            {
                return ListConversionGenerator(t, "Sequence");
            }
            case TupleType { Parameters: [var t] }:
            {
                var generator = Create(t);
                return new ConversionGenerator(TypeReflection.CreateGenericType("ValueTuple", [generator.TypeSyntax]),
                                               TypeReflection.CreateGenericType("Tuple", [generator.TypeSyntax, generator.ImporterTypeSyntax]));
            }
            case TupleType { Parameters: { Length: > 1 and <= 10 } ts }:
            {
                var generators = ImmutableArray.CreateRange(from t in ts select Create(t));
                return new ConversionGenerator(TupleType(SeparatedList(from item in generators select TupleElement(item.TypeSyntax))),
                                               TypeReflection.CreateGenericType("Tuple", [.. from item in generators select item.TypeSyntax, .. from item in generators select item.ImporterTypeSyntax]));
            }
            case VariadicTupleType { Of: var t }:
            {
                var generator = Create(t);
                return new ConversionGenerator(TypeReflection.CreateGenericType(nameof(ImmutableArray<object>), [generator.TypeSyntax]),
                                               TypeReflection.CreateGenericType("VarTuple", [generator.TypeSyntax, generator.ImporterTypeSyntax]));
            }
            case DictType { Key: var kt, Value: var vt }:
            {
                return DictionaryConversionGenerator(kt, vt, "Dictionary");
            }
            case MappingType { Key: var kt, Value: var vt }:
            {
                return DictionaryConversionGenerator(kt, vt, "Mapping");
            }
            case GeneratorType { Yield: var yt, Send: var st, Return: var rt }:
            {
                var generator = (Yield: Create(yt), Send: Create(st), Return: Create(rt));
                return new ConversionGenerator(TypeReflection.CreateGenericType("IGeneratorIterator",
                                               [
                                                   generator.Yield.TypeSyntax,
                                                   generator.Send.TypeSyntax,
                                                   generator.Return.TypeSyntax
                                               ]),
                                               TypeReflection.CreateGenericType("Generator",
                                               [
                                                   generator.Yield.TypeSyntax,
                                                   generator.Send.TypeSyntax,
                                                   generator.Return.TypeSyntax,
                                                   generator.Yield.ImporterTypeSyntax,
                                                   generator.Return.ImporterTypeSyntax
                                               ]));
            }
            case CoroutineType { Yield: NoneType, Send: NoneType, Return: var rt }:
            {
                return new CoroutineConversionGenerator(rt);
            }
            case AwaitableType { Of: var t }:
            {
                var generator = Create(t);
                return new ConversionGenerator(TypeReflection.CreateGenericType("IAwaitable", [generator.TypeSyntax]),
                                               TypeReflection.CreateGenericType("Awaitable", [generator.TypeSyntax, generator.ImporterTypeSyntax]));
            }
            case var other:
            {
                var typeSyntax = TypeReflection.AsPredefinedType(other, TypeReflection.ConversionDirection.FromPython).First(); // TODO: Investigate union
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

        public IEnumerable<StatementSyntax> GenerateCode(string inputName, string outputName,
                                                         string cancellationTokenName) =>
            [ParseStatement($"var {outputName} = {inputName}.BareImportAs<{TypeSyntax}, {ImporterTypeSyntax}>();")];
    }

    public static IResultConversionCodeGenerator ScalarConversionGenerator(SyntaxKind syntaxKind, string importerTypeName) =>
        ScalarConversionGenerator(PredefinedType(Token(syntaxKind)), importerTypeName);

    public static IResultConversionCodeGenerator ScalarConversionGenerator(TypeSyntax syntax, string importerTypeName) =>
        new ConversionGenerator(syntax, IdentifierName(importerTypeName));

    public static IResultConversionCodeGenerator OptionalConversionGenerator(PythonTypeSpec ofTypeSpec, string importerTypeName = "Optional")
    {
        var generator = Create(ofTypeSpec);
        return new ConversionGenerator(NullableType(generator.TypeSyntax),
                                       TypeReflection.CreateGenericType(importerTypeName, [generator.TypeSyntax, generator.ImporterTypeSyntax]));
    }

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

    sealed class CoroutineConversionGenerator : IResultConversionCodeGenerator
    {
        public CoroutineConversionGenerator(PythonTypeSpec returnTypeSpec)
        {
            var generator = Create(returnTypeSpec);
            TypeSyntax = TypeReflection.CreateGenericType("IAwaitable", [generator.TypeSyntax]);
            ImporterTypeSyntax = QualifiedName(ImportersQualifiedName, TypeReflection.CreateGenericType("Awaitable", [generator.TypeSyntax, generator.ImporterTypeSyntax]));
        }

        public TypeSyntax TypeSyntax { get; }
        public TypeSyntax ImporterTypeSyntax { get; }

        public IEnumerable<StatementSyntax> GenerateCode(string inputName, string outputName,
                                                         string cancellationTokenName) =>
            [ParseStatement($"using var {inputName}_Awaitable = {inputName}.BareImportAs<{TypeSyntax}, {ImporterTypeSyntax}>();"),
             ParseStatement($"var {outputName} = {inputName}_Awaitable.WaitAsync(dispose: true, {cancellationTokenName});")];
    }
}
