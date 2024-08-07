using CSnakes.Runtime.Python;
using System.ComponentModel;

namespace CSnakes.Runtime.Tests.Converter;

[Collection("ConversionTests")]
public class BoolConverterTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TestBoolBidirectional(bool input)
    {
        TypeConverter td = TypeDescriptor.GetConverter(typeof(PyObject));

        Assert.True(td.CanConvertFrom(typeof(bool)));

        var pyObj = td.ConvertFrom(input) as PyObject;

        Assert.NotNull(pyObj);

        Assert.True(td.CanConvertTo(typeof(bool)));

        // Convert back
        var str = td.ConvertTo(pyObj, typeof(bool));
        Assert.Equal(input, str);
    }
}
