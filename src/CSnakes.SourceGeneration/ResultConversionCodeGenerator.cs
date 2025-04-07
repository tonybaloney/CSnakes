using CSnakes.Parser.Types;
using CSnakes.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Globalization;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSnakes.SourceGeneration;

internal sealed class ResultConversionCodeGeneratorContext(IEnumerator<string> names)
{
    public ResultConversionCodeGeneratorContext() : this(GenerateNames()) { }

    public IEnumerator<string> Names { get; } = names;

    public string GenerateName() => Names.Read();

    private static IEnumerator<string> GenerateNames()
    {
        for (var i = 1;; i++)
            yield return FormattableString.Invariant($"__tmp_{i}");
    }
}

internal interface IResultConversionCodeGenerator
{
    TypeSyntax TypeSyntax { get; }

    IEnumerable<StatementSyntax> GenerateCode(ResultConversionCodeGeneratorContext context,
                                              string inputName, string outputName);
}

internal static partial class ResultConversionCodeGenerator
{
    public static IEnumerable<StatementSyntax> GenerateCode(this IResultConversionCodeGenerator generator,
                                                            string inputName, string outputName) =>
        generator.GenerateCode(new ResultConversionCodeGeneratorContext(), inputName, outputName);
}

partial class ResultConversionCodeGenerator
{
    private const string InternalServices = "global::CSnakes.Runtime.Python.InternalServices";

    private static readonly ScalarConversionGenerator Long = new(SyntaxKind.LongKeyword, "ConvertToInt64");
    private static readonly ScalarConversionGenerator String = new(SyntaxKind.StringKeyword, "ConvertToString");
    private static readonly ScalarConversionGenerator Boolean = new(SyntaxKind.BoolKeyword, "ConvertToBoolean");
    private static readonly ScalarConversionGenerator Double = new(SyntaxKind.DoubleKeyword, "ConvertToDouble");
    private static readonly ScalarConversionGenerator ByteArray = new(ParseTypeName("byte[]"),"ConvertToByteArray");

    public static IEnumerable<StatementSyntax> GenerateCode(PythonTypeSpec pythonTypeSpec,
                                                            string inputName, string outputName) =>
        Create(pythonTypeSpec).GenerateCode(inputName, outputName);

    public static IResultConversionCodeGenerator Create(PythonTypeSpec pythonTypeSpec) =>
        pythonTypeSpec switch
        {
            { Name: "int" } => Long,
            { Name: "str" } => String,
            { Name: "float" } => Double,
            { Name: "bool" } => Boolean,
            { Name: "bytes" } => ByteArray,
            { Name:  "tuple" or "typing.Tuple" or "Tuple", Arguments: var args } =>
                new TupleConversionGenerator([..from arg in args select Create(arg)]),
            { Name: "list" or "typing.List" or "List", Arguments: [var t] } =>
                new ListConversionGenerator(Create(t)),
            { Name: "typing.Sequence" or "Sequence", Arguments: [var t] } =>
                new ListConversionGenerator(Create(t), "ConvertSequenceToList"),
            { Name: "typing.Coroutine" or "Coroutine", Arguments: [var yt, var st, var rt] } =>
                new CoroutineConversionGenerator(Create(yt), Create(st), Create(rt)),
            _ => new RuntimeConversionGenerator(TypeReflection.AsPredefinedType(pythonTypeSpec, TypeReflection.ConversionDirection.FromPython)),
        };

    private sealed class ScalarConversionGenerator(TypeSyntax syntax, string method) :
        IResultConversionCodeGenerator
    {
        public ScalarConversionGenerator(SyntaxKind syntaxKind, string method) :
            this(PredefinedType(Token(syntaxKind)), method) { }

        public TypeSyntax TypeSyntax { get; } = syntax;

        public IEnumerable<StatementSyntax> GenerateCode(ResultConversionCodeGeneratorContext context,
                                                         string inputName, string outputName) =>
            [ParseStatement($"var {outputName} = {InternalServices}.{method}({inputName});")];
    }

    private sealed class RuntimeConversionGenerator(TypeSyntax syntax) :
        IResultConversionCodeGenerator
    {
        public TypeSyntax TypeSyntax => syntax;

        public IEnumerable<StatementSyntax> GenerateCode(ResultConversionCodeGeneratorContext context,
                                                         string inputName, string outputName) =>
        [
            LocalDeclarationStatement(
                VariableDeclaration(TypeSyntax,
                    SingletonSeparatedList(
                        VariableDeclarator(Identifier(outputName))
                            .WithInitializer(
                                EqualsValueClause(
                                    InvocationExpression(
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName(inputName),
                                                GenericName(Identifier("As"), TypeArgumentList(SingletonSeparatedList(TypeSyntax))))))))))
        ];
    }

    private sealed class TupleConversionGenerator(ImmutableArray<IResultConversionCodeGenerator> items) :
        IResultConversionCodeGenerator
    {
        private TypeSyntax? cachedTypeSyntax;

        public TypeSyntax TypeSyntax => this.cachedTypeSyntax ??=
            TupleType(SeparatedList(from item in items
                                    select TupleElement(item.TypeSyntax)));

        public IEnumerable<StatementSyntax> GenerateCode(ResultConversionCodeGeneratorContext context,
                                                         string inputName, string outputName)
        {
            var conversions = ImmutableArray.CreateRange(
                from item in items.Select((e, i) => (Index: i, Converter: e))
                select new
                {
                    Index = item.Index.ToString(CultureInfo.InvariantCulture),
                    item.Converter,
                    InputName = context.GenerateName(),
                    OutputName = context.GenerateName(),
                }
                into item
                select new
                {
                    item.OutputName,
                    Statements = ImmutableArray.Create(
                    [
                        ParseStatement($"using var {item.InputName} = {InternalServices}.GetTupleItem({inputName}, {item.Index});"),
                        .. item.Converter.GenerateCode(context, item.InputName, item.OutputName)
                    ])
                });

            return
            [
                ParseStatement($"{InternalServices}.CheckTuple({inputName});"),
                .. from c in conversions from s in c.Statements select s,
                conversions switch
                {
                    [{ OutputName: var varName }] => ParseStatement($"var {outputName} = ValueTuple.Create({varName});"),
                    var multiple => ParseStatement($"var {outputName} = ({string.Join(", ", from c in multiple select c.OutputName)});")
                }
            ];
        }
    }

    private static LocalFunctionStatementSyntax
        CreateLocalConversionFunctionSyntax(ResultConversionCodeGeneratorContext context,
                                            IResultConversionCodeGenerator conversionGenerator) =>
        LocalFunctionStatement(conversionGenerator.TypeSyntax, Identifier(context.GenerateName()))
            .WithModifiers(TokenList(Token(SyntaxKind.StaticKeyword)))
            .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier("obj")).WithType(IdentifierName("PyObject")))))
            .WithBody(Block(conversionGenerator.GenerateCode(context, "obj", "result")
                                               .Concat([ReturnStatement(IdentifierName("result"))])));

    private sealed class ListConversionGenerator(IResultConversionCodeGenerator item, string method = "ConvertToList") :
        IResultConversionCodeGenerator
    {
        private TypeSyntax? cachedTypeSyntax;

        public TypeSyntax TypeSyntax => this.cachedTypeSyntax ??=
            GenericName(Identifier(nameof(IReadOnlyList<object>)),
                TypeArgumentList(SingletonSeparatedList(item.TypeSyntax)));

        public IEnumerable<StatementSyntax> GenerateCode(ResultConversionCodeGeneratorContext context,
                                                         string inputName, string outputName)
        {
            var conversionFunctionSyntax = CreateLocalConversionFunctionSyntax(context, item);
            yield return conversionFunctionSyntax;
            yield return ParseStatement($"var {outputName} = {InternalServices}.{method}<{item.TypeSyntax.ToFullString()}>({InternalServices}.Clone({inputName}), {conversionFunctionSyntax.Identifier.ValueText});");
        }
    }

    private sealed class CoroutineConversionGenerator(IResultConversionCodeGenerator yield,
                                                      IResultConversionCodeGenerator send,
                                                      IResultConversionCodeGenerator @return) :
        IResultConversionCodeGenerator
    {
        private TypeSyntax? cachedTypeSyntax;

        public TypeSyntax TypeSyntax => this.cachedTypeSyntax ??=
            TypeReflection.CreateGenericType("ICoroutine", [yield.TypeSyntax, send.TypeSyntax, @return.TypeSyntax]);

        public IEnumerable<StatementSyntax> GenerateCode(ResultConversionCodeGeneratorContext context,
                                                         string inputName, string outputName)
        {
            var conversionFunctionSyntax = CreateLocalConversionFunctionSyntax(context, yield);
            yield return conversionFunctionSyntax;
            yield return ParseStatement($"var {outputName} = {InternalServices}.ConvertToCoroutine<{yield.TypeSyntax}, {send.TypeSyntax}, {@return.TypeSyntax}>({inputName}, {conversionFunctionSyntax.Identifier.ValueText});");
        }
    }
}
