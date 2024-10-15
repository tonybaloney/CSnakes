using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Converter;
public class ConverterTestBase : RuntimeTestBase
{
    protected static void RunTest<T>(T input)
    {
        using PythonObject? pyObj = PythonObject.From(input);
        Assert.NotNull(pyObj);

        // Convert back
        T actual = pyObj.As<T>();
        Assert.Equal(input, actual);
    }
}
