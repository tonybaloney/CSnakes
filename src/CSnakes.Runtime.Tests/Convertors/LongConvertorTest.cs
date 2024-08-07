using CSnakes.Runtime.Convertors;

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
    public void TestLongBidirectional(long input)
    {
        var convertor = new LongConvertor();

        Assert.True(convertor.CanEncode(typeof(long)));

        var result = convertor.TryEncode(input, out var pyObj);

        Assert.True(result);
        Assert.NotNull(pyObj);
        Assert.Equal(input.ToString(), pyObj.ToString());
        // Convert back
        result = convertor.TryDecode(pyObj!, out var str);
        Assert.True(result);
        Assert.Equal(input, str);

        // Release object
        pyObj.Dispose();
    }
}
