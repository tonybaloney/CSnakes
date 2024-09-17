using CSnakes.Parser;
using CSnakes.Reflection;
using Superpower;

namespace CSnakes.Tests;

public class TypeReflectionTests
{
    [Theory]
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
    [InlineData("Optional[str]", "string")]
    [InlineData("Optional[int]", "long")]
    [InlineData("Callable[[str], int]", "PyObject")]
    public void AsPredefinedTypeOldTypeNames(string pythonType, string expectedType) =>
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
        var result = PythonParser.PythonTypeDefinitionTokenizer.TryParse(tokens);
        Assert.True(result.HasValue, result.ToString());
        Assert.NotNull(result.Value);
        var reflectedType = TypeReflection.AsPredefinedType(result.Value);
        Assert.Equal(expectedType, reflectedType.ToString());
    }
}
