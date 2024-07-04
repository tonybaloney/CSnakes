using Microsoft.CodeAnalysis.CSharp.Syntax;
using PythonSourceGenerator.Reflection;

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
        var reflectedType = TypeReflection.AsPredefinedType(pythonType);
        Assert.Equal(expectedType, reflectedType.ToString());
    }

    [Fact]
    public void TestSplitTypeArgs()
    {
        Assert.Equal(TypeReflection.SplitTypeArgs("a"), ["a"]);
        Assert.Equal(TypeReflection.SplitTypeArgs("a,b"), ["a", "b"]);
        Assert.Equal(TypeReflection.SplitTypeArgs("a,   b, c"), ["a", "b", "c"]);
    }

    [Fact]
    public void TestSplitNestedTypeArgs()
    {
        Assert.Equal(TypeReflection.SplitTypeArgs("a"), ["a"]);
        Assert.Equal(TypeReflection.SplitTypeArgs("str, list[x]"), ["str", "list[x]"]);
        Assert.Equal(TypeReflection.SplitTypeArgs("str, int, int, tuple[str, str], str"), ["str", "int", "int", "tuple[str, str]", "str"]);
    }
}
