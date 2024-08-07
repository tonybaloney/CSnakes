using CSnakes.Runtime.Convertors;

namespace CSnakes.Runtime.Tests.Convertors;
public class UnicodeConvertorTests
{
    [Theory]
    [InlineData("Hello, World!")]
    [InlineData("你好，世界！")]
    [InlineData("こんにちは、世界！")]
    [InlineData("안녕하세요, 세계!")]
    [InlineData("مرحبا بالعالم!")]
    [InlineData("नमस्ते दुनिया!")]
    public void TestUnicodeBidirectional(string input)
    {
        var convertor = new StringConvertor();

        Assert.True(convertor.CanEncode(typeof(string)));

        var result = convertor.TryEncode(input, out var pyObj);

        Assert.True(result);
        Assert.NotNull(pyObj);

        // Convert back
        result = convertor.TryDecode(pyObj!, out var str);
        Assert.True(result);
        Assert.Equal(input, str);
    }
}
