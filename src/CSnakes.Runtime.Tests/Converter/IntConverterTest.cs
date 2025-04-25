using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Converter;

public class IntConverterTest :
    ConverterTestBase<int, IntConverterTest>,
    IConverterTestCasesContainer<int>
{
    public static TheoryData<int> TestCases => new()
    {
        0,
        1,
        -1,
        42,
        -42,
        int.MaxValue,
        int.MinValue,
    };

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void TestSignedIntFidelity(int input)
    {
        using var obj = PyObject.From(input);
        Assert.Equal(input, obj.As<long>());
    }

    [Theory]
    [InlineData(int.MaxValue + 1L)]
    [InlineData(int.MinValue - 1L)]
    public void TestOverflow(long input)
    {
        using var n = PyObject.From(input);
        void Act() => _ = n.As<int>();
        _ = Assert.Throws<OverflowException>(Act);
    }
}
