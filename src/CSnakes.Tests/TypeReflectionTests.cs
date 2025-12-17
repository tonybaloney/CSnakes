using CSnakes.Parser;
using CSnakes.Parser.Types;
using CSnakes.Reflection;
using Superpower;

namespace CSnakes.Tests;

public class TypeReflectionTests
{
    /// <summary>
    /// Generates test data including both plain and Annotated variations of the input data.
    /// </summary>
    private static TheoryData<string, T> PairAnnotated<T>(params IEnumerable<(string Input, T Expected)> data) =>
        new(from datum in data
            from variation in new[] { datum, datum with { Input = $"Annotated[{datum.Input}, 42]" } }
            select variation);

    public static TheoryData<string, string> AsPredefinedTypeData => PairAnnotated(
        ("None", "PyObject"),
        ("int", "long"),
        ("str", "string"),
        ("float", "double"),
        ("bool", "bool"),
        ("list[int]", "IReadOnlyList<long>"),
        ("list[str]", "IReadOnlyList<string>"),
        ("list[float]", "IReadOnlyList<double>"),
        ("list[bool]", "IReadOnlyList<bool>"),
        ("list[object]", "IReadOnlyList<PyObject>"),
        ("tuple[int, int]", "(long,long)"),
        ("tuple[str, str]", "(string,string)"),
        ("tuple[float, float]", "(double,double)"),
        ("tuple[bool, bool]", "(bool,bool)"),
        ("tuple[str, Any]", "(string,PyObject)"),
        ("tuple[str, list[int]]", "(string,IReadOnlyList<long>)"),
        ("dict[str, int]", "IReadOnlyDictionary<string,long>"),
        ("tuple[int, int, tuple[int, int]]", "(long,long,(long,long))"),
        ("None | str", "string?"),
        ("None | int", "long?"),
        ("str | None", "string?"),
        ("int | None", "long?"),
        ("list[int | None]", "IReadOnlyList<long?>"),
        ("None | list[int | None]", "IReadOnlyList<long?>?"));

    [Theory]
    [MemberData(nameof(AsPredefinedTypeData))]
    public void AsPredefinedType(string pythonType, string expectedType) =>
        ParsingTestInternal(pythonType, expectedType);

    public static TheoryData<string, string> AsPredefinedTypeOldTypeNamesData => PairAnnotated(
        ("List[int]", "IReadOnlyList<long>"),
        ("List[str]", "IReadOnlyList<string>"),
        ("List[float]", "IReadOnlyList<double>"),
        ("List[bool]", "IReadOnlyList<bool>"),
        ("List[object]", "IReadOnlyList<PyObject>"),
        ("Tuple[int, int]", "(long,long)"),
        ("Tuple[str, str]", "(string,string)"),
        ("Tuple[float, float]", "(double,double)"),
        ("Tuple[bool, bool]", "(bool,bool)"),
        ("Tuple[str, Any]", "(string,PyObject)"),
        ("Tuple[str, list[int]]", "(string,IReadOnlyList<long>)"),
        ("Dict[str, int]", "IReadOnlyDictionary<string,long>"),
        ("Tuple[int, int, Tuple[int, int]]", "(long,long,(long,long))"),
        ("Optional[str]", "string?"),
        ("Optional[int]", "long?"),
        ("Callable[[str], int]", "PyObject"),
        ("Literal['foo']", "PyObject"),
        ("Literal['bar', 1, 0x0, 3.14]", "PyObject"));

    [Theory]
    [MemberData(nameof(AsPredefinedTypeOldTypeNamesData))]
    public void AsPredefinedTypeOldTypeNames(string pythonType, string expectedType) =>
        ParsingTestInternal(pythonType, expectedType);

    public static TheoryData<string, string> AsCallableData => PairAnnotated(
        ("Callable[[str], int]", "PyObject"),
        ("Callable[[], int]", "PyObject"));

    [Theory]
    [MemberData(nameof(AsCallableData))]
    public void AsCallable(string pythonType, string expectedType) =>
        ParsingTestInternal(pythonType, expectedType);

    public static TheoryData<string, string> TupleParsingTestData => PairAnnotated(
        ("tuple[str]", "ValueTuple<string>"),
        ("tuple[str, str]", "(string,string)"),
        ("tuple[str, str, str]", "(string,string,string)"),
        ("tuple[str, str, str, str]", "(string,string,string,string)"),
        ("tuple[str, str, str, str, str]", "(string,string,string,string,string)"),
        ("tuple[str, str, str, str, str, str]", "(string,string,string,string,string,string)"),
        ("tuple[str, str, str, str, str, str, str]", "(string,string,string,string,string,string,string)"),
        ("tuple[str, str, str, str, str, str, str, str]", "(string,string,string,string,string,string,string,string)"),
        ("tuple[str, str, str, str, str, str, str, str, str]", "(string,string,string,string,string,string,string,string,string)"),
        ("tuple[str, str, str, str, str, str, str, str, str, str]", "(string,string,string,string,string,string,string,string,string,string)"),
        ("tuple[str, str, str, str, str, str, str, str, str, str, str]", "(string,string,string,string,string,string,string,string,string,string,string)"),
        ("tuple[str, str, str, str, str, str, str, str, str, str, str, str]", "(string,string,string,string,string,string,string,string,string,string,string,string)"),
        ("tuple[str, str, str, str, str, str, str, str, str, str, str, str, str]", "(string,string,string,string,string,string,string,string,string,string,string,string,string)"),
        ("tuple[str, str, str, str, str, str, str, str, str, str, str, str, str, str]", "(string,string,string,string,string,string,string,string,string,string,string,string,string,string)"),
        ("tuple[str, str, str, str, str, str, str, str, str, str, str, str, str, str, str]", "(string,string,string,string,string,string,string,string,string,string,string,string,string,string,string)"),
        ("tuple[str, str, str, str, str, str, str, str, str, str, str, str, str, str, str, str]", "(string,string,string,string,string,string,string,string,string,string,string,string,string,string,string,string)"));

    [Theory]
    [MemberData(nameof(TupleParsingTestData))]
    public void TupleParsingTest(string pythonType, string expectedType) =>
        ParsingTestInternal(pythonType, expectedType);

    private static void ParsingTestInternal(string pythonType, string expectedType)
    {
        var tokens = PythonTokenizer.Instance.Tokenize(pythonType);
        var result = PythonParser.PythonTypeDefinitionParser.TryParse(tokens);
        Assert.True(result.HasValue, result.ToString());
        Assert.NotNull(result.Value);
        var reflectedType = TypeReflection.AsPredefinedType(result.Value, TypeReflection.ConversionDirection.FromPython).First();
        Assert.Equal(expectedType, reflectedType.ToString());
    }

    public static TheoryData<string, IReadOnlyList<string>> UnionParsingTestData =>
        PairAnnotated<IReadOnlyList<string>>(
            ("int | str", ["long", "string"]),
            ("int | str | bool", ["long", "string", "bool"]));

    [Theory]
    [MemberData(nameof(UnionParsingTestData))]
    public void UnionParsingTest(string pythonType, IReadOnlyList<string> expectedTypes)
    {
        var tokens = PythonTokenizer.Instance.Tokenize(pythonType);
        var result = PythonParser.PythonTypeDefinitionParser.TryParse(tokens);
        Assert.True(result.HasValue, result.ToString());
        Assert.NotNull(result.Value);
        Assert.Equal("Union", result.Value.Name);
        var reflectedTypes = TypeReflection.AsPredefinedType(result.Value, TypeReflection.ConversionDirection.ToPython);

        Assert.Equal(expectedTypes.Count, reflectedTypes.Count());
        for (int i = 0; i < expectedTypes.Count; i++)
        {
            Assert.Equal(expectedTypes[i], reflectedTypes.ElementAt(i).ToString());
        }
    }

    [Theory]
    [InlineData("int")]
    [InlineData("str | None")]
    [InlineData("Foo | None")]
    [InlineData("int | str | None")]
    [InlineData("AnyStr")]
    [InlineData("AnyStr | None")]
    [InlineData("tuple[str] | None")]
    public void UnionParsingTestFromPythonAlwaysSingle(string pythonType)
    {
        var tokens = PythonTokenizer.Instance.Tokenize(pythonType);
        var result = PythonParser.PythonTypeDefinitionParser.TryParse(tokens);
        Assert.True(result.HasValue, result.ToString());
        Assert.NotNull(result.Value);
        var reflectedTypes = TypeReflection.AsPredefinedType(result.Value, TypeReflection.ConversionDirection.FromPython);
        Assert.Single(reflectedTypes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("list[")]
    [InlineData("list[]")]
    [InlineData("[]")]
    [InlineData("Callable[int]")]
    [InlineData("Callable[int, int]")]
    [InlineData("Callable[int, int, int]")]
    [InlineData("Callable[int, [int, int]]")]
    [InlineData("Literal")] // Literal must have arguments
    [InlineData("Literal[]")]
    public void InvalidParsingTest(string pythonType)
    {
        var tokens = PythonTokenizer.Instance.Tokenize(pythonType);
        var result = PythonParser.PythonTypeDefinitionParser.TryParse(tokens);
        Assert.False(result.HasValue);
    }

    [Theory]
    [InlineData("int | None")]
    [InlineData("None | int")]
    public void UnionNoneTest(string pythonType)
    {
        var tokens = PythonTokenizer.Instance.Tokenize(pythonType);
        var result = PythonParser.PythonTypeDefinitionParser.TryParse(tokens);
        Assert.True(result.Remainder.IsAtEnd);
        Assert.True(result.HasValue, result.ToString());
        var optional = Assert.IsType<OptionalType>(result.Value);
        Assert.IsType<IntType>(optional.Of);
    }

    [Theory]
    // Single type in Union collapses to the type itself
    [InlineData("Union[int]", "int")]
    // Duplicate types in Union are deduplicated
    [InlineData("Union[int, int]", "int")]
    // Two distinct types in Union
    [InlineData("Union[int, str]", "Union[int, str]")]
    // Three distinct types in Union
    [InlineData("Union[int, str, bool]", "Union[int, str, bool]")]
    // Nested Union is flattened
    [InlineData("Union[Union[int, str], bool]", "Union[int, str, bool]")]
    [InlineData("Union[int, Union[str, bool]]", "Union[int, str, bool]")]
    [InlineData("Union[Union[int, str, bool]]", "Union[int, str, bool]")]
    [InlineData("Union[Union[int, Union[str, bool]]]", "Union[int, str, bool]")]
    [InlineData("Union[Union[int], Union[str], Union[bool]]", "Union[int, str, bool]")]
    // Union with None becomes Optional
    [InlineData("Union[None, int]", "Optional[int]")]
    [InlineData("Union[int, None]", "Optional[int]")]
    [InlineData("Union[None, int, int]", "Optional[int]")]
    [InlineData("Union[None, int, None]", "Optional[int]")]
    [InlineData("Union[None, Optional[int]]", "Optional[int]")]
    [InlineData("Union[Optional[int], None]", "Optional[int]")]
    // Union with None and other non-None types (does not become Optional)
    [InlineData("Union[None, int, str]", "Union[NoneType, int, str]")]
    // Large Union with duplicates and None
    [InlineData("Union[None, bool, int, float, str, object, bytes, bytearray, int, int]", "Union[NoneType, bool, int, float, str, object, bytes, bytearray]")]
    // Pipe syntax...
    // Duplicates are deduplicated
    [InlineData("int | int", "int")]
    // Two distinct types
    [InlineData("int | str", "Union[int, str]")]
    // Three distinct types
    [InlineData("int | str | bool", "Union[int, str, bool]")]
    // Mixed with Union
    [InlineData("Union[int, str] | bool", "Union[int, str, bool]")]
    [InlineData("int | Union[str, bool]", "Union[int, str, bool]")]
    // Multiple Unions get collapsed
    [InlineData("Union[int] | Union[str] | Union[bool]", "Union[int, str, bool]")]
    // None and type (expressed in various ways) becomes Optional
    [InlineData("None | int", "Optional[int]")]
    [InlineData("int | None", "Optional[int]")]
    [InlineData("None | int | int", "Optional[int]")]
    [InlineData("None | int | None", "Optional[int]")]
    [InlineData("None | Optional[int]", "Optional[int]")]
    [InlineData("Optional[int] | None", "Optional[int]")]
    // None, type, and another type (does not become Optional)
    [InlineData("None | int | str", "Union[NoneType, int, str]")]
    // Large union with duplicates and None
    [InlineData("None | bool | int | float | str | object | bytes | bytearray | int | int", "Union[NoneType, bool, int, float, str, object, bytes, bytearray]")]
    // Union with pipe syntax inside is still a Union
    [InlineData("Union[int | str | bool]", "Union[int, str, bool]")]
    public void UnionNormalizationTest(string input, string expected)
    {
        var tokens = PythonTokenizer.Instance.Tokenize(input);
        var result = PythonParser.PythonTypeDefinitionParser.TryParse(tokens);
        Assert.True(result.Remainder.IsAtEnd);
        Assert.True(result.HasValue, result.ToString());
        Assert.NotNull(result.Value);
        Assert.Equal(expected, result.Value.ToString());
    }
}
