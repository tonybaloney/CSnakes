using CSnakes.Runtime.Python;
using System.ComponentModel;

namespace CSnakes.Runtime.Tests.Converter;

public class DoubleConverterTest : RuntimeTestBase
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
        using (GIL.Acquire())
        {
            using PyObject? pyObj = td.ConvertFrom(input) as PyObject;

            Assert.NotNull(pyObj);

            Assert.True(td.CanConvertTo(typeof(double)));

            // Convert back
            object? str = td.ConvertTo(pyObj, typeof(double));
            Assert.Equal(input, str);
        }
    }
}
