namespace CSnakes.Runtime.Tests.Converter;

public class UnicodeConverterTest : ConverterTestBase
{
    [Theory]
    [InlineData("Hello, World!")]
    [InlineData("你好，世界！")]
    [InlineData("こんにちは、世界！")]
    [InlineData("안녕하세요, 세계!")]
    [InlineData("مرحبا بالعالم!")]
    [InlineData("नमस्ते दुनिया!")]
    public void TestUnicodeBidirectional(string input) => RunTest(input);
}
