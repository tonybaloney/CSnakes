using PythonSourceGenerator.Parser;
using PythonSourceGenerator.Reflection;
using Superpower;

namespace PythonSourceGenerator.Tests;

public class TypeReflectionTests
{
    [Theory]
    [InlineData("int", "long")]
    [InlineData("str", "string")]
    [InlineData("float", "double")]
    [InlineData("bool", "bool")]
    [InlineData("list[int]", "IEnumerable<long>")]
    [InlineData("list[str]", "IEnumerable<string>")]
    [InlineData("list[float]", "IEnumerable<double>")]
    [InlineData("list[bool]", "IEnumerable<bool>")]
    [InlineData("list[object]", "IEnumerable<PyObject>")]
    [InlineData("tuple[int, int]", "Tuple<long,long>")]
    [InlineData("tuple[str, str]", "Tuple<string,string>")]
    [InlineData("tuple[float, float]", "Tuple<double,double>")]
    [InlineData("tuple[bool, bool]", "Tuple<bool,bool>")]
    [InlineData("tuple[str, Any]", "Tuple<string,PyObject>")]
    [InlineData("tuple[str, list[int]]", "Tuple<string,IEnumerable<long>>")]
    [InlineData("dict[str, int]", "IReadOnlyDictionary<string,long>")]
    [InlineData("tuple[int, int, tuple[int, int]]", "Tuple<long,long,Tuple<long,long>>")]
    public void AsPredefinedType(string pythonType, string expectedType)
    {
        var tokens = PythonSignatureTokenizer.Instance.Tokenize(pythonType);
        var result = PythonSignatureParser.PythonTypeDefinitionTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        var reflectedType = TypeReflection.AsPredefinedType(result.Value);
        Assert.Equal(expectedType, reflectedType.ToString());
    }

    [Theory]
    [InlineData("List[int]", "IEnumerable<long>")]
    [InlineData("List[str]", "IEnumerable<string>")]
    [InlineData("List[float]", "IEnumerable<double>")]
    [InlineData("List[bool]", "IEnumerable<bool>")]
    [InlineData("List[object]", "IEnumerable<PyObject>")]
    [InlineData("Tuple[int, int]", "Tuple<long,long>")]
    [InlineData("Tuple[str, str]", "Tuple<string,string>")]
    [InlineData("Tuple[float, float]", "Tuple<double,double>")]
    [InlineData("Tuple[bool, bool]", "Tuple<bool,bool>")]
    [InlineData("Tuple[str, Any]", "Tuple<string,PyObject>")]
    [InlineData("Tuple[str, list[int]]", "Tuple<string,IEnumerable<long>>")]
    [InlineData("Dict[str, int]", "IReadOnlyDictionary<string,long>")]
    [InlineData("Tuple[int, int, Tuple[int, int]]", "Tuple<long,long,Tuple<long,long>>")]
    public void AsPredefinedTypeOldTypeNames(string pythonType, string expectedType)
    {
        var tokens = PythonSignatureTokenizer.Instance.Tokenize(pythonType);
        var result = PythonSignatureParser.PythonTypeDefinitionTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        var reflectedType = TypeReflection.AsPredefinedType(result.Value);
        Assert.Equal(expectedType, reflectedType.ToString());
    }
}
