using CSnakes.Runtime.Convertors;

namespace CSnakes.Runtime.Tests.Convertors;

[Collection("ConversionTests")]
public class DoubleConvertorTest
{
    [Theory]
    [InlineData(0.0)]
    [InlineData(2.0)]
    [InlineData(-2.0)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(double.PositiveInfinity)]
    public void TestDoubleBidirectional(double input)
    {
        var convertor = new DoubleConvertor();

        Assert.True(convertor.CanEncode(typeof(double)));

        var result = convertor.TryEncode(input, out var pyObj);

        Assert.True(result);
        Assert.NotNull(pyObj);

        // Convert back
        result = convertor.TryDecode(pyObj!, out var str);
        Assert.True(result);
        Assert.Equal(input, str);

        // Release object
        pyObj.Dispose();
    }
}
