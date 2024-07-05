using PythonSourceGenerator.Parser;
using PythonSourceGenerator.Reflection;
using Superpower;

namespace PythonSourceGenerator.Tests;

public class TypeReflectionTests
{
    [Theory]
    [InlineData("int", "long", "ToInt64")]
    [InlineData("str", "string", "ToString")]
    [InlineData("float", "double", "ToDouble")]
    [InlineData("bool", "bool", "ToBoolean")]
    [InlineData("list[int]", "IEnumerable<long>", "AsEnumerable<long>")]
    [InlineData("list[str]", "IEnumerable<string>", "AsEnumerable<string>")]
    [InlineData("list[float]", "IEnumerable<double>", "AsEnumerable<double>")]
    [InlineData("list[bool]", "IEnumerable<bool>", "AsEnumerable<bool>")]
    [InlineData("list[object]", "IEnumerable<PyObject>", "AsEnumerable<PyObject>")]
    [InlineData("tuple[int, int]", "Tuple<long,long>", "AsTuple<long, long>")]
    [InlineData("tuple[str, str]", "Tuple<string,string>", "AsTuple<string, string>")]
    [InlineData("tuple[float, float]", "Tuple<double,double>", "AsTuple<double, double>")]
    [InlineData("tuple[bool, bool]", "Tuple<bool,bool>", "AsTuple<bool, bool>")]
    [InlineData("tuple[str, Any]", "Tuple<string,PyObject>", "AsTuple<string, PyObject>")]
    [InlineData("tuple[str, list[int]]", "Tuple<string,IEnumerable<long>>", "AsTuple<string, IEnumerable<long>>")]
    [InlineData("dict[str, int]", "IReadOnlyDictionary<string,long>", "AsDictionary<string, long>")]
    [InlineData("tuple[int, int, tuple[int, int]]", "Tuple<long,long,Tuple<long,long>>", "AsTuple<long, long, Tuple<long,long>>")]
    public void AsPredefinedType(string pythonType, string expectedType, string expectedConvertor)
    {
        var tokens = PythonSignatureTokenizer.Instance.Tokenize(pythonType);
        var result = PythonSignatureParser.PythonTypeDefinitionTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        var reflectedType = TypeReflection.AsPredefinedType(result.Value);
        Assert.Equal(expectedType, reflectedType.Syntax.ToString());
        Assert.Equal(expectedConvertor, reflectedType.Convertor);
    }
}
