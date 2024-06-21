using PythonSourceGenerator.Reflection;

namespace PythonSourceGenerator.Tests
{
    public class TypeReflectionTests
    {
        [Theory]
        [InlineData("int", "long", "ToInt64")]
        [InlineData("str", "string", "ToString")]
        [InlineData("float", "double", "ToDouble")]
        [InlineData("bool", "bool", "ToBoolean")]
        [InlineData("list[int]", "List<long>", "AsList<long>")]
        [InlineData("list[str]", "List<string>", "AsList<string>")]
        [InlineData("list[float]", "List<double>", "AsList<double>")]
        [InlineData("list[bool]", "List<bool>", "AsList<bool>")]
        [InlineData("list[object]", "List<PyObject>", "AsList<PyObject>")]
        [InlineData("tuple[int, int]", "Tuple<long,long>", "AsTuple<long, long>")]
        [InlineData("tuple[str, str]", "Tuple<string,string>", "AsTuple<string, string>")]
        [InlineData("tuple[float, float]", "Tuple<double,double>", "AsTuple<double, double>")]
        [InlineData("tuple[bool, bool]", "Tuple<bool,bool>", "AsTuple<bool, bool>")]
        [InlineData("tuple[str, Any]", "Tuple<string,PyObject>", "AsTuple<string, PyObject>")]
        public void AsPredefinedType(string pythonType, string expectedType, string expectedConvertor)
        {
            var actualType = TypeReflection.AsPredefinedType(pythonType, out var actualConvertor).ToString();
            Assert.Equal(expectedType, actualType);
            Assert.Equal(expectedConvertor, actualConvertor);
        }
    }
}
