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
    [InlineData("tuple[int, int, tuple[int, int]]", "Tuple<long,long,Tuple<long,long>>", "AsTuple<long, long, Tuple<long,long>>")]
    public void AsPredefinedType(string pythonType, string expectedType, string expectedConvertor)
    {
        var reflectedType = TypeReflection.AsPredefinedType(pythonType);
        Assert.Equal(expectedType, reflectedType.Syntax.ToString());
        Assert.Equal(expectedConvertor, reflectedType.Convertor);
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
