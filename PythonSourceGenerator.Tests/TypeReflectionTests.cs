using Microsoft.CodeAnalysis.CSharp.Syntax;
using PythonSourceGenerator.Reflection;

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
    public void AsPredefinedType(string pythonType, string expectedType, string expectedConvertor)
    {
        (string convertor, TypeSyntax syntax) = TypeReflection.AsPredefinedType(pythonType);
        string actualType = syntax.ToString();
        Assert.Equal(expectedType, actualType);
        Assert.Equal(expectedConvertor, convertor);
    }
}
