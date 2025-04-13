using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Converter;

public class IntConverterTest : ConverterTestBase
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(42)]
    [InlineData(-42)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void TestIntBidirectional(int input) => RunTest(input);

    [Theory]
    [InlineData(int.MaxValue + 1L)]
    [InlineData(int.MinValue - 1L)]
    public void TestOverflow(long input)
    {
        void Act() => _ = PyObject.From(input).As<int>();
        _ = Assert.Throws<OverflowException>(Act);
    }
}
