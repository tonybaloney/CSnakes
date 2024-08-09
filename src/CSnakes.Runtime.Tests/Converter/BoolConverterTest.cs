using CSnakes.Runtime.Python;
using System.ComponentModel;

namespace CSnakes.Runtime.Tests.Converter;

public class BoolConverterTest : ConverterTestBase
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TestBoolBidirectional(bool input)
    {
        TypeConverter td = TypeDescriptor.GetConverter(typeof(PyObject));

        Assert.True(td.CanConvertFrom(typeof(bool)));

        using PyObject? pyObj = td.ConvertFrom(input) as PyObject;

        Assert.NotNull(pyObj);

        Assert.True(td.CanConvertTo(typeof(bool)));

        // Convert back
        object? str = td.ConvertTo(pyObj, typeof(bool));
        Assert.Equal(input, str);
    }
}
