namespace CSnakes.Runtime.Tests.Converter;

public class DoubleConverterTest : ConverterTestBase
{
    [Theory]
    [InlineData(0.0)]
    [InlineData(2.0)]
    [InlineData(-2.0)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(double.PositiveInfinity)]
    public void TestDoubleBidirectional(double input) => RunTest(input);

    [Theory]
    [InlineData(0.0)]
    [InlineData(2.0)]
    [InlineData(-2.0)]
    [InlineData(float.NegativeInfinity)]
    [InlineData(float.PositiveInfinity)]
    public void TestFloatBidirectional(float input) => RunTest(input);
}
