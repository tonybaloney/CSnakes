using CSnakes.Runtime.Python;
using System.ComponentModel;

namespace CSnakes.Runtime.Tests.Convertors;

[Collection("ConversionTests")]
public class LongConvertorTest
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(42)]
    [InlineData(-42)]
    [InlineData(long.MaxValue)]
    [InlineData(long.MinValue)]
    public void TestLongBidirectional(long input)
    {
        var td = TypeDescriptor.GetConverter(typeof(PyObject));

        Assert.True(td.CanConvertFrom(typeof(long)));

        var pyObj = td.ConvertFrom(input) as PyObject;

        Assert.NotNull(pyObj);
        Assert.Equal(input.ToString(), pyObj.ToString());

        // Convert back
        var str = td.ConvertTo(pyObj, typeof(long));
        Assert.Equal(input, str);
    }
}
