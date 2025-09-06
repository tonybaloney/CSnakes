using CSnakes.Parser;
using CSnakes.Parser.Types;
using Superpower;
using Superpower.Model;

namespace CSnakes.Tests;

public class PythonTypeDefinitionParserTests
{
    private static TokenList<PythonToken> Tokenize(string input) => PythonTokenizer.Instance.Tokenize(input);

    private static T TestParse<T>(string input) where T : PythonTypeSpec
    {
        var result = PythonParser.PythonTypeDefinitionParser.TryParse(Tokenize(input));
        Assert.True(result.Remainder.IsAtEnd);
        Assert.True(result.HasValue, result.ToString());
        return Assert.IsType<T>(result.Value);
    }


    private static void TestParseError(string input, string expectedErrorMessage)
    {
        var tokens = Tokenize(input);
        void Act() => _ = PythonParser.PythonTypeDefinitionParser.Parse(tokens);
        var exception = Assert.Throws<ParseException>(Act);
        Assert.Equal(expectedErrorMessage, exception.Message);
    }

    [Fact] public void NoneTest()  => _ = TestParse<NoneType>("None");
    [Fact] public void AnyTest()   => _ = TestParse<AnyType>("Any");
    [Fact] public void IntTest()   => _ = TestParse<IntType>("int");
    [Fact] public void StrTest()   => _ = TestParse<StrType>("str");
    [Fact] public void BoolTest()  => _ = TestParse<BoolType>("bool");
    [Fact] public void FloatTest() => _ = TestParse<FloatType>("float");
    [Fact] public void BytesTest() => _ = TestParse<BytesType>("bytes");

    [Theory]
    [InlineData("Buffer")]
    [InlineData("collections.abc.Buffer")]
    public void BufferTest(string input) =>
        _ = TestParse<BufferType>(input);

    [Theory]
    [InlineData("Optional[int]")]
    [InlineData("typing.Optional[int]")]
    public void OptionalTest(string input)
    {
        var optional = TestParse<OptionalType>(input);
        _ = Assert.IsType<IntType>(optional.Of);
    }

    [Theory]
    [InlineData("Optional", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("typing.Optional", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("Optional[]", "Syntax error (line 1, column 10): unexpected `]`, expected Type Definition.")]
    [InlineData("Optional[int, int]", "Syntax error (line 1, column 13): unexpected `,`, expected `]`.")]
    public void OptionalArgTest(string input, string expectedErrorMessage) =>
        TestParseError(input, expectedErrorMessage);

    [Theory]
    [InlineData("list", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("List", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("typing.List", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("list[]", "Syntax error (line 1, column 6): unexpected `]`, expected Type Definition.")]
    [InlineData("list[int, str]", "Syntax error (line 1, column 9): unexpected `,`, expected `]`.")]
    public void ListArgTest(string input, string expectedErrorMessage) =>
        TestParseError(input, expectedErrorMessage);

    [Theory]
    [InlineData("Sequence", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("typing.Sequence", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("Sequence[]", "Syntax error (line 1, column 10): unexpected `]`, expected Type Definition.")]
    [InlineData("Sequence[int, str]", "Syntax error (line 1, column 13): unexpected `,`, expected `]`.")]
    public void SequenceArgTest(string input, string expectedErrorMessage) =>
        TestParseError(input, expectedErrorMessage);

    [Theory]
    [InlineData("dict", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("Dict", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("typing.Dict", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("dict[]", "Syntax error (line 1, column 6): unexpected `]`, expected Type Definition.")]
    [InlineData("dict[int]", "Syntax error (line 1, column 9): unexpected `]`, expected `,`.")]
    [InlineData("dict[int, str, bool]", "Syntax error (line 1, column 14): unexpected `,`, expected `]`.")]
    public void DictArgTest(string input, string expectedErrorMessage) =>
        TestParseError(input, expectedErrorMessage);

    [Theory]
    [InlineData("Mapping", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("typing.Mapping", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("Mapping[]", "Syntax error (line 1, column 9): unexpected `]`, expected Type Definition.")]
    [InlineData("Mapping[int]", "Syntax error (line 1, column 12): unexpected `]`, expected `,`.")]
    [InlineData("Mapping[int, str, bool]", "Syntax error (line 1, column 17): unexpected `,`, expected `]`.")]
    public void MappingArgTest(string input, string expectedErrorMessage) =>
        TestParseError(input, expectedErrorMessage);

    [Theory]
    [InlineData("Generator", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("typing.Generator", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("Generator[]", "Syntax error (line 1, column 11): unexpected `]`, expected Type Definition.")]
    [InlineData("Generator[int]", "Syntax error (line 1, column 14): unexpected `]`, expected `,`.")]
    [InlineData("Generator[int, str]", "Syntax error (line 1, column 19): unexpected `]`, expected `,`.")]
    [InlineData("Generator[int, str, bool, float]", "Syntax error (line 1, column 25): unexpected `,`, expected `]`.")]
    public void GeneratorArgTest(string input, string expectedErrorMessage) =>
        TestParseError(input, expectedErrorMessage);

    [Theory]
    [InlineData("Coroutine", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("typing.Coroutine", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("collections.abc.Coroutine", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("Coroutine[]", "Syntax error (line 1, column 11): unexpected `]`, expected Type Definition.")]
    [InlineData("Coroutine[int]", "Syntax error (line 1, column 14): unexpected `]`, expected `,`.")]
    [InlineData("Coroutine[int, str]", "Syntax error (line 1, column 19): unexpected `]`, expected `,`.")]
    [InlineData("Coroutine[int, str, bool, float]", "Syntax error (line 1, column 25): unexpected `,`, expected `]`.")]
    public void CoroutineArgTest(string input, string expectedErrorMessage) =>
        TestParseError(input, expectedErrorMessage);

    [Theory]
    [InlineData("Callable", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("typing.Callable", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("collections.abc.Callable", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("Callable[]", "Syntax error (line 1, column 10): unexpected `]`, expected `[`.")]
    [InlineData("Callable[int]", "Syntax error (line 1, column 10): unexpected identifier `int`, expected `[`.")]
    [InlineData("Callable[[int]]", "Syntax error (line 1, column 15): unexpected `]`, expected `,`.")]
    public void CallableArgTest(string input, string expectedErrorMessage) =>
        TestParseError(input, expectedErrorMessage);

    [Theory]
    [InlineData("Literal", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("typing.Literal", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("Literal[]", "Syntax error (line 1, column 9): unexpected `]`, expected Constant.")]
    public void LiteralArgTest(string input, string expectedErrorMessage) =>
        TestParseError(input, expectedErrorMessage);

    [Theory]
    [InlineData("Union", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("typing.Union", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("Union[]", "Syntax error (line 1, column 7): unexpected `]`, expected Type Definition.")]
    public void UnionArgTest(string input, string expectedErrorMessage) =>
        TestParseError(input, expectedErrorMessage);

    [Theory]
    [InlineData("list[int]")]
    [InlineData("List[int]")]
    [InlineData("typing.List[int]")]
    public void ListTest(string input)
    {
        var type = TestParse<ListType>(input);
        _ = Assert.IsType<IntType>(type.Of);
    }

    [Theory]
    [InlineData("Sequence[str]")]
    [InlineData("typing.Sequence[str]")]
    [InlineData("collections.abc.Sequence[str]")]
    public void SequenceTest(string input)
    {
        var type = TestParse<SequenceType>(input);
        _ = Assert.IsType<StrType>(type.Of);
    }

    [Theory]
    [InlineData("dict[str, int]")]
    [InlineData("Dict[str, int]")]
    [InlineData("typing.Dict[str, int]")]
    public void DictTest(string input)
    {
        var type = TestParse<DictType>(input);
        _ = Assert.IsType<StrType>(type.Key);
        _ = Assert.IsType<IntType>(type.Value);
    }

    [Theory]
    [InlineData("Mapping[str, float]")]
    [InlineData("typing.Mapping[str, float]")]
    [InlineData("collections.abc.Mapping[str, float]")]
    public void MappingTest(string input)
    {
        var type = TestParse<MappingType>(input);
        _ = Assert.IsType<StrType>(type.Key);
        _ = Assert.IsType<FloatType>(type.Value);
    }

    [Theory]
    [InlineData("Generator[int, str, bool]")]
    [InlineData("typing.Generator[int, str, bool]")]
    [InlineData("collections.abc.Generator[int, str, bool]")]
    public void GeneratorTest(string input)
    {
        var type = TestParse<GeneratorType>(input);
        _ = Assert.IsType<IntType>(type.Yield);
        _ = Assert.IsType<StrType>(type.Send);
        _ = Assert.IsType<BoolType>(type.Return);
    }

    [Theory]
    [InlineData("Coroutine[int, str, bool]")]
    [InlineData("typing.Coroutine[int, str, bool]")]
    [InlineData("collections.abc.Coroutine[int, str, bool]")]
    public void CoroutineTest(string input)
    {
        var type = TestParse<CoroutineType>(input);
        _ = Assert.IsType<IntType>(type.Yield);
        _ = Assert.IsType<StrType>(type.Send);
        _ = Assert.IsType<BoolType>(type.Return);
    }

    [Theory]
    [InlineData("Callable[[int, str], bool]")]
    [InlineData("typing.Callable[[int, str], bool]")]
    [InlineData("collections.abc.Callable[[int, str], bool]")]
    public void CallableTest(string input)
    {
        var type = TestParse<CallbackType>(input);
        Assert.Equal(2, type.Parameters.Length);
        _ = Assert.IsType<IntType>(type.Parameters[0]);
        _ = Assert.IsType<StrType>(type.Parameters[1]);
        _ = Assert.IsType<BoolType>(type.Return);
    }

    [Theory]
    [InlineData("Callable[[], None]")]
    [InlineData("typing.Callable[[], None]")]
    public void CallableNoParametersTest(string input)
    {
        var type = TestParse<CallbackType>(input);
        Assert.Empty(type.Parameters);
        _ = Assert.IsType<NoneType>(type.Return);
    }

    [Theory]
    [InlineData("Literal[1]")]
    [InlineData("typing.Literal[1]")]
    public void LiteralIntTest(string input)
    {
        var type = TestParse<LiteralType>(input);
        _ = Assert.Single(type.Constants);
        var constant = Assert.IsType<PythonConstant.Integer>(type.Constants[0]);
        Assert.Equal(1, constant.Value);
    }

    [Theory]
    [InlineData("Literal['hello']")]
    [InlineData("typing.Literal['hello']")]
    public void LiteralStringTest(string input)
    {
        var type = TestParse<LiteralType>(input);
        _ = Assert.Single(type.Constants);
        var constant = Assert.IsType<PythonConstant.String>(type.Constants[0]);
        Assert.Equal("hello", constant.Value);
    }

    [Theory]
    [InlineData("Literal[True, False]")]
    [InlineData("typing.Literal[True, False]")]
    public void LiteralMultipleTest(string input)
    {
        var type = TestParse<LiteralType>(input);
        Assert.Equal(2, type.Constants.Length);
        var trueConstant = Assert.IsType<PythonConstant.Bool>(type.Constants[0]);
        Assert.True(trueConstant.Value);
        var falseConstant = Assert.IsType<PythonConstant.Bool>(type.Constants[1]);
        Assert.False(falseConstant.Value);
    }

    [Theory]
    [InlineData("Union[int, str]")]
    [InlineData("typing.Union[int, str]")]
    public void UnionTest(string input)
    {
        var type = TestParse<UnionType>(input);
        Assert.Equal(2, type.Choices.Length);
        _ = Assert.IsType<IntType>(type.Choices[0]);
        _ = Assert.IsType<StrType>(type.Choices[1]);
    }

    [Theory]
    [InlineData("int | str")]
    [InlineData("str | int")]
    public void UnionPipeTest(string input)
    {
        var type = TestParse<UnionType>(input);
        Assert.Equal(2, type.Choices.Length);
    }

    [Theory]
    [InlineData("Union[int, None]")]
    [InlineData("None | int")]
    [InlineData("int | None")]
    public void UnionWithNoneBecomesOptionalTest(string input)
    {
        var type = TestParse<OptionalType>(input);
        _ = Assert.IsType<IntType>(type.Of);
    }

    [Theory]
    [InlineData("tuple[int, str]")]
    [InlineData("Tuple[int, str]")]
    [InlineData("typing.Tuple[int, str]")]
    public void TupleTest(string input)
    {
        var type = TestParse<TupleType>(input);
        Assert.Equal(2, type.Parameters.Length);
        _ = Assert.IsType<IntType>(type.Parameters[0]);
        _ = Assert.IsType<StrType>(type.Parameters[1]);
    }

    [Theory]
    [InlineData("tuple[int]")]
    [InlineData("Tuple[int]")]
    public void TupleSingleParameterTest(string input)
    {
        var type = TestParse<TupleType>(input);
        _ = Assert.Single(type.Parameters);
        _ = Assert.IsType<IntType>(type.Parameters[0]);
    }

    [Theory]
    [InlineData("CustomType")]
    [InlineData("my_module.CustomType")]
    [InlineData("package.module.CustomType")]
    public void ParsePythonTypeSpecTest(string input)
    {
        var type = TestParse<ParsedPythonTypeSpec>(input);
        Assert.Equal(input, type.Name);
        Assert.Empty(type.Arguments);
    }

    [Theory]
    [InlineData("CustomType[int]")]
    [InlineData("my_module.CustomType[str, bool]")]
    public void ParsePythonTypeSpecWithArgumentsTest(string input)
    {
        var type = TestParse<ParsedPythonTypeSpec>(input);
        Assert.True(type.Arguments.Length > 0);
    }

    [Theory]
    [InlineData("MyGeneric[int, str]")]
    public void GenericTypeWithMultipleArgumentsTest(string input)
    {
        var type = TestParse<ParsedPythonTypeSpec>(input);
        Assert.Equal("MyGeneric", type.Name);
        Assert.Equal(2, type.Arguments.Length);
        _ = Assert.IsType<IntType>(type.Arguments[0]);
        _ = Assert.IsType<StrType>(type.Arguments[1]);
    }

    [Theory]
    [InlineData("Union[int, str, float]")]
    [InlineData("int | str | float")]
    public void UnionThreeTypesTest(string input)
    {
        var type = TestParse<UnionType>(input);
        Assert.Equal(3, type.Choices.Length);
        _ = Assert.IsType<IntType>(type.Choices[0]);
        _ = Assert.IsType<StrType>(type.Choices[1]);
        _ = Assert.IsType<FloatType>(type.Choices[2]);
    }

    [Fact]
    public void NestedGenericsTest()
    {
        var type = TestParse<ListType>("list[dict[str, int]]");
        var innerType = Assert.IsType<DictType>(type.Of);
        _ = Assert.IsType<StrType>(innerType.Key);
        _ = Assert.IsType<IntType>(innerType.Value);
    }

    [Fact]
    public void ComplexNestedTypeTest()
    {
        var type = TestParse<OptionalType>("Optional[Union[list[str], dict[int, bool]]]");
        var unionType = Assert.IsType<UnionType>(type.Of);
        Assert.Equal(2, unionType.Choices.Length);

        var listType = Assert.IsType<ListType>(unionType.Choices[0]);
        _ = Assert.IsType<StrType>(listType.Of);

        var dictType = Assert.IsType<DictType>(unionType.Choices[1]);
        _ = Assert.IsType<IntType>(dictType.Key);
        _ = Assert.IsType<BoolType>(dictType.Value);
    }
}
