namespace CSnakes.Runtime.Tests.Converter;

public class BytesConverterTest : ConverterTestBase
{
    [Theory]
    [InlineData(new byte[]{ 0x01, 0x02, 0x03, 0x04 })]
    [InlineData(new byte[] { })]
    public void TestBytesBidirectional(byte[] input) => RunTest(input);
}
