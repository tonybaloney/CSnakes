namespace CSnakes.Runtime.Tests.Converter;

public class BoolConverterTest : ConverterTestBase
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TestBoolBidirectional(bool input) => RunTest(input);
}
