using CSnakes.Parser;
using CSnakes.Reflection;
using Superpower;

namespace CSnakes.Tests;

public class TypeReflectionTests
{
    [Theory]
    [InlineData("None", "PyObject")]
    [InlineData("int", "long")]
    [InlineData("str", "string")]
    [InlineData("float", "double")]
    [InlineData("bool", "bool")]
    [InlineData("list[int]", "IReadOnlyList<long>")]
    [InlineData("list[str]", "IReadOnlyList<string>")]
    [InlineData("list[float]", "IReadOnlyList<double>")]
    [InlineData("list[bool]", "IReadOnlyList<bool>")]
    [InlineData("list[object]", "IReadOnlyList<PyObject>")]
    [InlineData("tuple[int, int]", "(long,long)")]
    [InlineData("tuple[str, str]", "(string,string)")]
    [InlineData("tuple[float, float]", "(double,double)")]
    [InlineData("tuple[bool, bool]", "(bool,bool)")]
    [InlineData("tuple[str, Any]", "(string,PyObject)")]
    [InlineData("tuple[str, list[int]]", "(string,IReadOnlyList<long>)")]
    [InlineData("dict[str, int]", "IReadOnlyDictionary<string,long>")]
    [InlineData("tuple[int, int, tuple[int, int]]", "(long,long,(long,long))")]
    [InlineData("None | str", "string?")]
    [InlineData("None | int", "long?")]
    [InlineData("str | None", "string?")]
    [InlineData("int | None", "long?")]
    [InlineData("list[int | None]", "IReadOnlyList<long?>")]
    [InlineData("None | list[int | None]", "IReadOnlyList<long?>?")]
    public void AsPredefinedType(string pythonType, string expectedType) =>
        ParsingTestInternal(pythonType, expectedType);

    [Theory]
    [InlineData("List[int]", "IReadOnlyList<long>")]
    [InlineData("List[str]", "IReadOnlyList<string>")]
    [InlineData("List[float]", "IReadOnlyList<double>")]
    [InlineData("List[bool]", "IReadOnlyList<bool>")]
    [InlineData("List[object]", "IReadOnlyList<PyObject>")]
    [InlineData("Tuple[int, int]", "(long,long)")]
    [InlineData("Tuple[str, str]", "(string,string)")]
    [InlineData("Tuple[float, float]", "(double,double)")]
    [InlineData("Tuple[bool, bool]", "(bool,bool)")]
    [InlineData("Tuple[str, Any]", "(string,PyObject)")]
    [InlineData("Tuple[str, list[int]]", "(string,IReadOnlyList<long>)")]
    [InlineData("Dict[str, int]", "IReadOnlyDictionary<string,long>")]
    [InlineData("Tuple[int, int, Tuple[int, int]]", "(long,long,(long,long))")]
    [InlineData("Optional[str]", "string?")]
    [InlineData("Optional[int]", "long?")]
    [InlineData("Callable[[str], int]", "PyObject")]
    [InlineData("Literal['foo']", "PyObject")]
    [InlineData("Literal['bar', 1, 0x0, 3.14]", "PyObject")]
    public void AsPredefinedTypeOldTypeNames(string pythonType, string expectedType) =>
        ParsingTestInternal(pythonType, expectedType);

    [Theory]
    [InlineData("Callable[[str], int]", "PyObject")]
    [InlineData("Callable[[], int]", "PyObject")]
    public void AsCallable(string pythonType, string expectedType) =>
        ParsingTestInternal(pythonType, expectedType);

    [Theory]
    [InlineData("tuple[str]", "ValueTuple<string>")]
    [InlineData("tuple[str, str]", "(string,string)")]
    [InlineData("tuple[str, str, str]", "(string,string,string)")]
    [InlineData("tuple[str, str, str, str]", "(string,string,string,string)")]
    [InlineData("tuple[str, str, str, str, str]", "(string,string,string,string,string)")]
    [InlineData("tuple[str, str, str, str, str, str]", "(string,string,string,string,string,string)")]
    [InlineData("tuple[str, str, str, str, str, str, str]", "(string,string,string,string,string,string,string)")]
    [InlineData("tuple[str, str, str, str, str, str, str, str]", "(string,string,string,string,string,string,string,string)")]
    [InlineData("tuple[str, str, str, str, str, str, str, str, str]", "(string,string,string,string,string,string,string,string,string)")]
    [InlineData("tuple[str, str, str, str, str, str, str, str, str, str]", "(string,string,string,string,string,string,string,string,string,string)")]
    [InlineData("tuple[str, str, str, str, str, str, str, str, str, str, str]", "(string,string,string,string,string,string,string,string,string,string,string)")]
    [InlineData("tuple[str, str, str, str, str, str, str, str, str, str, str, str]", "(string,string,string,string,string,string,string,string,string,string,string,string)")]
    [InlineData("tuple[str, str, str, str, str, str, str, str, str, str, str, str, str]", "(string,string,string,string,string,string,string,string,string,string,string,string,string)")]
    [InlineData("tuple[str, str, str, str, str, str, str, str, str, str, str, str, str, str]", "(string,string,string,string,string,string,string,string,string,string,string,string,string,string)")]
    [InlineData("tuple[str, str, str, str, str, str, str, str, str, str, str, str, str, str, str]", "(string,string,string,string,string,string,string,string,string,string,string,string,string,string,string)")]
    [InlineData("tuple[str, str, str, str, str, str, str, str, str, str, str, str, str, str, str, str]", "(string,string,string,string,string,string,string,string,string,string,string,string,string,string,string,string)")]
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

    [Theory]
    [InlineData("int | str", "long", "string")]
    [InlineData("int | str | bool", "long", "string", "bool")]
    public void UnionParsingTest(string pythonType, params string[] expectedTypes)
    {
        var tokens = PythonTokenizer.Instance.Tokenize(pythonType);
        var result = PythonParser.PythonTypeDefinitionParser.TryParse(tokens);
        Assert.True(result.HasValue, result.ToString());
        Assert.NotNull(result.Value);
        Assert.Equal("Union", result.Value.Name);
        var reflectedTypes = TypeReflection.AsPredefinedType(result.Value, TypeReflection.ConversionDirection.FromPython);

        Assert.Equal(expectedTypes.Length, reflectedTypes.Count());
        for (int i = 0; i < expectedTypes.Length; i++)
        {
            Assert.Equal(expectedTypes[i], reflectedTypes.ElementAt(i).ToString());
        }
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
        Assert.NotNull(result.Value);
        Assert.Equal("Optional",result.Value.Name);
        var arg = Assert.Single(result.Value.Arguments);
        Assert.Equal("int", arg.Name);
        Assert.Empty(arg.Arguments);
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
    [InlineData("Union[None, int, str]", "Union[None, int, str]")]
    // Large Union with duplicates and None
    [InlineData("Union[None, bool, int, float, str, object, bytes, bytearray, int, int]", "Union[None, bool, int, float, str, object, bytes, bytearray]")]
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
    [InlineData("None | int | str", "Union[None, int, str]")]
    // Large union with duplicates and None
    [InlineData("None | bool | int | float | str | object | bytes | bytearray | int | int", "Union[None, bool, int, float, str, object, bytes, bytearray]")]
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
