using CSnakes.Runtime.Convertors;

namespace CSnakes.Runtime.Tests.Convertors;

[Collection("ConversionTests")]
public class BoolConvertorTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TestBoolBidirectional(bool input)
    {
        var convertor = new BoolConvertor();

        Assert.True(convertor.CanEncode(typeof(bool)));

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
