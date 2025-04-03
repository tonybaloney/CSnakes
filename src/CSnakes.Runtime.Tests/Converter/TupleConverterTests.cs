namespace CSnakes.Runtime.Tests.Converter;

public class TupleConverterTests : ConverterTestBase
{
    [Fact]
    public void TupleConverter_SingleArgument()
    {
        Tuple<long> input = new(42);
        RunTest(input);
    }

    [Fact]
    public void TupleConverter_TwoArguments()
    {
        (long, long) input = (42, 42);
        RunTest(input);
    }

    [Fact]
    public void TupleConverter_ThreeArguments()
    {
        (long, long, long) input = (42, 42, 42);
        RunTest(input);
    }

    [Fact]
    public void TupleConverter_EightArguments()
    {
        (long, long, long, long, long, long, long, long) input = (1, 2, 3, 4, 5, 6, 7, 8);
        RunTest(input);
    }

    [Fact]
    public void TupleConverter_SeventeenArguments()
    {
        (long, long, long, long, long, long, long, long, long, long, long, long, long, long, long, long, long) input =
            (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17);
        RunTest(input);
    }
}
