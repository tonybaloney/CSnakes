namespace CSnakes.Runtime.Tests.Converter;

public class LongConverterTest : ConverterTestBase
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(42)]
    [InlineData(-42)]
    [InlineData(long.MaxValue)]
    [InlineData(long.MinValue)]
    public void TestLongBidirectional(long input) => RunTest(input);
}
