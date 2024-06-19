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
        [InlineData("list[object]", "List<object>", "AsList<object>")]
        public void AsPredefinedType(string pythonType, string expectedType, string expectedConvertor)
        {
            var actualType = TypeReflection.AsPredefinedType(pythonType, out var actualConvertor).ToString();
            Assert.Equal(expectedType, actualType);
            Assert.Equal(expectedConvertor, actualConvertor);
        }
    }
}
