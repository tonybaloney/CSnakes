using CSnakes.Parser;
using CSnakes.Parser.Types;
using Superpower;

namespace CSnakes.Tests;

public class PythonTypeDefinitionParserTests
{
    private static PythonTypeSpec TestParse(string pythonType)
    {
        var tokens = PythonTokenizer.Instance.Tokenize(pythonType);
        var result = PythonParser.PythonTypeDefinitionParser.TryParse(tokens);
        Assert.True(result.Remainder.IsAtEnd);
        Assert.True(result.HasValue, result.ToString());
        return result.Value;
    }

    private static void TestParseError(string pythonType, string expectedErrorMessage)
    {
        var tokens = PythonTokenizer.Instance.Tokenize(pythonType);
        void Act() => _ = PythonParser.PythonTypeDefinitionParser.Parse(tokens);
        var exception = Assert.Throws<ParseException>(Act);
        Assert.Equal(expectedErrorMessage, exception.Message);
    }

    [Fact]
    public void NoneTest() =>
        Assert.IsType<NoneType>(TestParse("None"));

    [Fact]
    public void AnyTest() =>
        Assert.IsType<AnyType>(TestParse("Any"));

    [Fact]
    public void IntTest() =>
        Assert.IsType<IntType>(TestParse("int"));

    [Fact]
    public void StrTest() =>
        Assert.IsType<StrType>(TestParse("str"));

    [Fact]
    public void BoolTest() =>
        Assert.IsType<BoolType>(TestParse("bool"));

    [Fact]
    public void FloatTest() =>
        Assert.IsType<FloatType>(TestParse("float"));

    [Theory]
    [InlineData("Buffer")]
    [InlineData("collections.abc.Buffer")]
    public void BufferTest(string pythonType) =>
        Assert.IsType<BufferType>(TestParse(pythonType));

    [Theory]
    [InlineData("Optional[int]")]
    [InlineData("typing.Optional[int]")]
    public void OptionalTest(string pythonType)
    {
        var optional = Assert.IsType<OptionalType>(TestParse(pythonType));
        _ = Assert.IsType<IntType>(optional.Of);
    }

    [Theory]
    [InlineData("Optional", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("typing.Optional", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("Optional[]", "Syntax error (line 1, column 10): unexpected `]`, expected Type Definition.")]
    [InlineData("Optional[int, int]", "Syntax error (line 1, column 13): unexpected `,`, expected `]`.")]
    public void OptionalArgTest(string pythonType, string expectedErrorMessage) =>
        TestParseError(pythonType, expectedErrorMessage);

    [Theory]
    [InlineData("list", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("List", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("typing.List", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("list[]", "Syntax error (line 1, column 6): unexpected `]`, expected Type Definition.")]
    [InlineData("list[int, str]", "Syntax error (line 1, column 9): unexpected `,`, expected `]`.")]
    public void ListArgTest(string pythonType, string expectedErrorMessage) =>
        TestParseError(pythonType, expectedErrorMessage);

    [Theory]
    [InlineData("Sequence", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("typing.Sequence", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("Sequence[]", "Syntax error (line 1, column 10): unexpected `]`, expected Type Definition.")]
    [InlineData("Sequence[int, str]", "Syntax error (line 1, column 13): unexpected `,`, expected `]`.")]
    public void SequenceArgTest(string pythonType, string expectedErrorMessage) =>
        TestParseError(pythonType, expectedErrorMessage);

    [Theory]
    [InlineData("dict", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("Dict", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("typing.Dict", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("dict[]", "Syntax error (line 1, column 6): unexpected `]`, expected Type Definition.")]
    [InlineData("dict[int]", "Syntax error (line 1, column 9): unexpected `]`, expected `,`.")]
    [InlineData("dict[int, str, bool]", "Syntax error (line 1, column 14): unexpected `,`, expected `]`.")]
    public void DictArgTest(string pythonType, string expectedErrorMessage) =>
        TestParseError(pythonType, expectedErrorMessage);

    [Theory]
    [InlineData("Mapping", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("typing.Mapping", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("Mapping[]", "Syntax error (line 1, column 9): unexpected `]`, expected Type Definition.")]
    [InlineData("Mapping[int]", "Syntax error (line 1, column 12): unexpected `]`, expected `,`.")]
    [InlineData("Mapping[int, str, bool]", "Syntax error (line 1, column 17): unexpected `,`, expected `]`.")]
    public void MappingArgTest(string pythonType, string expectedErrorMessage) =>
        TestParseError(pythonType, expectedErrorMessage);

    [Theory]
    [InlineData("Generator", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("typing.Generator", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("Generator[]", "Syntax error (line 1, column 11): unexpected `]`, expected Type Definition.")]
    [InlineData("Generator[int]", "Syntax error (line 1, column 14): unexpected `]`, expected `,`.")]
    [InlineData("Generator[int, str]", "Syntax error (line 1, column 19): unexpected `]`, expected `,`.")]
    [InlineData("Generator[int, str, bool, float]", "Syntax error (line 1, column 25): unexpected `,`, expected `]`.")]
    public void GeneratorArgTest(string pythonType, string expectedErrorMessage) =>
        TestParseError(pythonType, expectedErrorMessage);

    [Theory]
    [InlineData("Coroutine", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("typing.Coroutine", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("collections.abc.Coroutine", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("Coroutine[]", "Syntax error (line 1, column 11): unexpected `]`, expected Type Definition.")]
    [InlineData("Coroutine[int]", "Syntax error (line 1, column 14): unexpected `]`, expected `,`.")]
    [InlineData("Coroutine[int, str]", "Syntax error (line 1, column 19): unexpected `]`, expected `,`.")]
    [InlineData("Coroutine[int, str, bool, float]", "Syntax error (line 1, column 25): unexpected `,`, expected `]`.")]
    public void CoroutineArgTest(string pythonType, string expectedErrorMessage) =>
        TestParseError(pythonType, expectedErrorMessage);

    [Theory]
    [InlineData("Callable", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("typing.Callable", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("collections.abc.Callable", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("Callable[]", "Syntax error (line 1, column 10): unexpected `]`, expected `[`.")]
    [InlineData("Callable[int]", "Syntax error (line 1, column 10): unexpected identifier `int`, expected `[`.")]
    [InlineData("Callable[[int]]", "Syntax error (line 1, column 15): unexpected `]`, expected `,`.")]
    public void CallableArgTest(string pythonType, string expectedErrorMessage) =>
        TestParseError(pythonType, expectedErrorMessage);

    [Theory]
    [InlineData("Literal", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("typing.Literal", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("Literal[]", "Syntax error (line 1, column 9): unexpected `]`, expected Constant.")]
    public void LiteralArgTest(string pythonType, string expectedErrorMessage) =>
        TestParseError(pythonType, expectedErrorMessage);

    [Theory]
    [InlineData("Union", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("typing.Union", "Syntax error: unexpected end of input, expected `[`.")]
    [InlineData("Union[]", "Syntax error (line 1, column 7): unexpected `]`, expected Type Definition.")]
    public void UnionArgTest(string pythonType, string expectedErrorMessage) =>
        TestParseError(pythonType, expectedErrorMessage);

    [Fact]
    public void BytesTest() =>
        Assert.IsType<BytesType>(TestParse("bytes"));

    [Theory]
    [InlineData("list[int]")]
    [InlineData("List[int]")]
    [InlineData("typing.List[int]")]
    public void ListTest(string pythonType)
    {
        var listType = Assert.IsType<ListType>(TestParse(pythonType));
        _ = Assert.IsType<IntType>(listType.Of);
    }

    [Theory]
    [InlineData("Sequence[str]")]
    [InlineData("typing.Sequence[str]")]
    public void SequenceTest(string pythonType)
    {
        var sequenceType = Assert.IsType<SequenceType>(TestParse(pythonType));
        _ = Assert.IsType<StrType>(sequenceType.Of);
    }

    [Theory]
    [InlineData("dict[str, int]")]
    [InlineData("Dict[str, int]")]
    [InlineData("typing.Dict[str, int]")]
    public void DictTest(string pythonType)
    {
        var dictType = Assert.IsType<DictType>(TestParse(pythonType));
        _ = Assert.IsType<StrType>(dictType.Key);
        _ = Assert.IsType<IntType>(dictType.Value);
    }

    [Theory]
    [InlineData("Mapping[str, float]")]
    [InlineData("typing.Mapping[str, float]")]
    public void MappingTest(string pythonType)
    {
        var mappingType = Assert.IsType<MappingType>(TestParse(pythonType));
        _ = Assert.IsType<StrType>(mappingType.Key);
        _ = Assert.IsType<FloatType>(mappingType.Value);
    }

    [Theory]
    [InlineData("Generator[int, str, bool]")]
    [InlineData("typing.Generator[int, str, bool]")]
    public void GeneratorTest(string pythonType)
    {
        var generatorType = Assert.IsType<GeneratorType>(TestParse(pythonType));
        _ = Assert.IsType<IntType>(generatorType.Yield);
        _ = Assert.IsType<StrType>(generatorType.Send);
        _ = Assert.IsType<BoolType>(generatorType.Return);
    }

    [Theory]
    [InlineData("Coroutine[int, str, bool]")]
    [InlineData("collections.abc.Coroutine[int, str, bool]")]
    [InlineData("typing.Coroutine[int, str, bool]")]
    public void CoroutineTest(string pythonType)
    {
        var coroutineType = Assert.IsType<CoroutineType>(TestParse(pythonType));
        _ = Assert.IsType<IntType>(coroutineType.Yield);
        _ = Assert.IsType<StrType>(coroutineType.Send);
        _ = Assert.IsType<BoolType>(coroutineType.Return);
    }

    [Theory]
    [InlineData("Callable[[int, str], bool]")]
    [InlineData("typing.Callable[[int, str], bool]")]
    [InlineData("collections.abc.Callable[[int, str], bool]")]
    public void CallableTest(string pythonType)
    {
        var callableType = Assert.IsType<CallbackType>(TestParse(pythonType));
        Assert.Equal(2, callableType.Parameters.Length);
        _ = Assert.IsType<IntType>(callableType.Parameters[0]);
        _ = Assert.IsType<StrType>(callableType.Parameters[1]);
        _ = Assert.IsType<BoolType>(callableType.Return);
    }

    [Theory]
    [InlineData("Callable[[], None]")]
    [InlineData("typing.Callable[[], None]")]
    public void CallableNoParametersTest(string pythonType)
    {
        var callableType = Assert.IsType<CallbackType>(TestParse(pythonType));
        Assert.Empty(callableType.Parameters);
        _ = Assert.IsType<NoneType>(callableType.Return);
    }

    [Theory]
    [InlineData("Literal[1]")]
    [InlineData("typing.Literal[1]")]
    public void LiteralIntTest(string pythonType)
    {
        var literalType = Assert.IsType<LiteralType>(TestParse(pythonType));
        Assert.Single(literalType.Constants);
        var constant = Assert.IsType<PythonConstant.Integer>(literalType.Constants[0]);
        Assert.Equal(1, constant.Value);
    }

    [Theory]
    [InlineData("Literal['hello']")]
    [InlineData("typing.Literal['hello']")]
    public void LiteralStringTest(string pythonType)
    {
        var literalType = Assert.IsType<LiteralType>(TestParse(pythonType));
        Assert.Single(literalType.Constants);
        var constant = Assert.IsType<PythonConstant.String>(literalType.Constants[0]);
        Assert.Equal("hello", constant.Value);
    }

    [Theory]
    [InlineData("Literal[True, False]")]
    [InlineData("typing.Literal[True, False]")]
    public void LiteralMultipleTest(string pythonType)
    {
        var literalType = Assert.IsType<LiteralType>(TestParse(pythonType));
        Assert.Equal(2, literalType.Constants.Length);
        var trueConstant = Assert.IsType<PythonConstant.Bool>(literalType.Constants[0]);
        Assert.True(trueConstant.Value);
        var falseConstant = Assert.IsType<PythonConstant.Bool>(literalType.Constants[1]);
        Assert.False(falseConstant.Value);
    }

    [Theory]
    [InlineData("Union[int, str]")]
    [InlineData("typing.Union[int, str]")]
    public void UnionTest(string pythonType)
    {
        var unionType = Assert.IsType<UnionType>(TestParse(pythonType));
        Assert.Equal(2, unionType.Choices.Length);
        _ = Assert.IsType<IntType>(unionType.Choices[0]);
        _ = Assert.IsType<StrType>(unionType.Choices[1]);
    }

    [Theory]
    [InlineData("int | str")]
    [InlineData("str | int")]
    public void UnionPipeTest(string pythonType)
    {
        var unionType = Assert.IsType<UnionType>(TestParse(pythonType));
        Assert.Equal(2, unionType.Choices.Length);
    }

    [Theory]
    [InlineData("Union[int, None]")]
    [InlineData("None | int")]
    [InlineData("int | None")]
    public void UnionWithNoneBecomesOptionalTest(string pythonType)
    {
        var optionalType = Assert.IsType<OptionalType>(TestParse(pythonType));
        _ = Assert.IsType<IntType>(optionalType.Of);
    }

    [Theory]
    [InlineData("tuple[int, str]")]
    [InlineData("Tuple[int, str]")]
    [InlineData("typing.Tuple[int, str]")]
    public void TupleTest(string pythonType)
    {
        var tupleType = Assert.IsType<TupleType>(TestParse(pythonType));
        Assert.Equal(2, tupleType.Parameters.Length);
        _ = Assert.IsType<IntType>(tupleType.Parameters[0]);
        _ = Assert.IsType<StrType>(tupleType.Parameters[1]);
    }

    [Theory]
    [InlineData("tuple[int]")]
    [InlineData("Tuple[int]")]
    public void TupleSingleParameterTest(string pythonType)
    {
        var tupleType = Assert.IsType<TupleType>(TestParse(pythonType));
        Assert.Single(tupleType.Parameters);
        _ = Assert.IsType<IntType>(tupleType.Parameters[0]);
    }

    [Theory]
    [InlineData("CustomType")]
    [InlineData("my_module.CustomType")]
    [InlineData("package.module.CustomType")]
    public void GenericPythonTypeSpecTest(string pythonType)
    {
        var genericType = Assert.IsType<GenericPythonTypeSpec>(TestParse(pythonType));
        Assert.Equal(pythonType, genericType.Name);
        Assert.Empty(genericType.Arguments);
    }

    [Theory]
    [InlineData("CustomType[int]")]
    [InlineData("my_module.CustomType[str, bool]")]
    public void GenericPythonTypeSpecWithArgumentsTest(string pythonType)
    {
        var genericType = Assert.IsType<GenericPythonTypeSpec>(TestParse(pythonType));
        Assert.True(genericType.Arguments.Length > 0);
    }

    [Theory]
    [InlineData("MyGeneric[int, str]")]
    public void GenericTypeWithMultipleArgumentsTest(string pythonType)
    {
        var genericType = Assert.IsType<GenericPythonTypeSpec>(TestParse(pythonType));
        Assert.Equal("MyGeneric", genericType.Name);
        Assert.Equal(2, genericType.Arguments.Length);
        _ = Assert.IsType<IntType>(genericType.Arguments[0]);
        _ = Assert.IsType<StrType>(genericType.Arguments[1]);
    }

    [Theory]
    [InlineData("Union[int, str, float]")]
    [InlineData("int | str | float")]
    public void UnionThreeTypesTest(string pythonType)
    {
        var unionType = Assert.IsType<UnionType>(TestParse(pythonType));
        Assert.Equal(3, unionType.Choices.Length);
        _ = Assert.IsType<IntType>(unionType.Choices[0]);
        _ = Assert.IsType<StrType>(unionType.Choices[1]);
        _ = Assert.IsType<FloatType>(unionType.Choices[2]);
    }

    [Fact]
    public void NestedGenericsTest()
    {
        var result = TestParse("list[dict[str, int]]");
        var listType = Assert.IsType<ListType>(result);
        var dictType = Assert.IsType<DictType>(listType.Of);
        _ = Assert.IsType<StrType>(dictType.Key);
        _ = Assert.IsType<IntType>(dictType.Value);
    }

    [Fact]
    public void ComplexNestedTypeTest()
    {
        var result = TestParse("Optional[Union[list[str], dict[int, bool]]]");
        var optionalType = Assert.IsType<OptionalType>(result);
        var unionType = Assert.IsType<UnionType>(optionalType.Of);
        Assert.Equal(2, unionType.Choices.Length);

        var listType = Assert.IsType<ListType>(unionType.Choices[0]);
        _ = Assert.IsType<StrType>(listType.Of);

        var dictType = Assert.IsType<DictType>(unionType.Choices[1]);
        _ = Assert.IsType<IntType>(dictType.Key);
        _ = Assert.IsType<BoolType>(dictType.Value);
    }
}
