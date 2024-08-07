using CSnakes.Runtime.Python;
using System.ComponentModel;

namespace CSnakes.Runtime.Tests.Converter;

[Collection("ConversionTests")]
public class DoubleConverterTest
{
    [Theory]
    [InlineData(0.0)]
    [InlineData(2.0)]
    [InlineData(-2.0)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(double.PositiveInfinity)]
    public void TestDoubleBidirectional(double input)
    {
        var td = TypeDescriptor.GetConverter(typeof(PyObject));

        Assert.True(td.CanConvertFrom(typeof(double)));

        var pyObj = td.ConvertFrom(input) as PyObject;

        Assert.NotNull(pyObj);

        Assert.True(td.CanConvertTo(typeof(double)));

        // Convert back
        var str = td.ConvertTo(pyObj, typeof(double));
        Assert.Equal(input, str);
    }
}
