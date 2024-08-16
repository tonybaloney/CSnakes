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
    [InlineData("list[int]", "IReadOnlyCollection<long>")]
    [InlineData("list[str]", "IReadOnlyCollection<string>")]
    [InlineData("list[float]", "IReadOnlyCollection<double>")]
    [InlineData("list[bool]", "IReadOnlyCollection<bool>")]
    [InlineData("list[object]", "IReadOnlyCollection<PyObject>")]
    [InlineData("tuple[int, int]", "(long,long)")]
    [InlineData("tuple[str, str]", "(string,string)")]
    [InlineData("tuple[float, float]", "(double,double)")]
    [InlineData("tuple[bool, bool]", "(bool,bool)")]
    [InlineData("tuple[str, Any]", "(string,PyObject)")]
    [InlineData("tuple[str, list[int]]", "(string,IReadOnlyCollection<long>)")]
    [InlineData("dict[str, int]", "IReadOnlyDictionary<string,long>")]
    [InlineData("tuple[int, int, tuple[int, int]]", "(long,long,(long,long))")]
    public void AsPredefinedType(string pythonType, string expectedType) =>
        ParsingTestInternal(pythonType, expectedType);

    [Theory]
    [InlineData("List[int]", "IReadOnlyCollection<long>")]
    [InlineData("List[str]", "IReadOnlyCollection<string>")]
    [InlineData("List[float]", "IReadOnlyCollection<double>")]
    [InlineData("List[bool]", "IReadOnlyCollection<bool>")]
    [InlineData("List[object]", "IReadOnlyCollection<PyObject>")]
    [InlineData("Tuple[int, int]", "(long,long)")]
    [InlineData("Tuple[str, str]", "(string,string)")]
    [InlineData("Tuple[float, float]", "(double,double)")]
    [InlineData("Tuple[bool, bool]", "(bool,bool)")]
    [InlineData("Tuple[str, Any]", "(string,PyObject)")]
    [InlineData("Tuple[str, list[int]]", "(string,IReadOnlyCollection<long>)")]
    [InlineData("Dict[str, int]", "IReadOnlyDictionary<string,long>")]
    [InlineData("Tuple[int, int, Tuple[int, int]]", "(long,long,(long,long))")]
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
        Assert.True(result.HasValue);
        Assert.NotNull(result.Value);
        var reflectedType = TypeReflection.AsPredefinedType(result.Value);
        Assert.Equal(expectedType, reflectedType.ToString());
    }
}
