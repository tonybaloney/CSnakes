using System.Numerics;

namespace CSnakes.Runtime.Tests.Converter;

public class BigIntegerConverterTest :
    ConverterTestBase<BigInteger, BigIntegerConverterTest>,
    IConverterTestCasesContainer<BigInteger>
{
    public static TheoryData<BigInteger> TestCases => new()
    {
        BigInteger.Parse("12345678987654345678764345678987654345678765")
    };
}
