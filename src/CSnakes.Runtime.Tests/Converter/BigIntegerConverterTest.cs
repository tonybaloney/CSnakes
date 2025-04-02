using System.Numerics;

namespace CSnakes.Runtime.Tests.Converter;

public class BigIntegerConverterTest : ConverterTestBase
{
    [Fact]
    public void TestVeryBigNumbers()
    {
        const string number = "12345678987654345678764345678987654345678765";
        // Something that is too big for a long (I8)
        BigInteger input = BigInteger.Parse(number);

        RunTest(input);
    }
}
