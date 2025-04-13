using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Converter;
public class LongConverterTest :
    ConverterTestBase<long, PyObjectImporters.Int64, LongConverterTest>,
    IConverterTestCasesContainer<long>
{
    public static TheoryData<long> TestCases => new()
    {
        0,
        1,
        -1,
        42,
        -42,
        long.MaxValue,
        long.MinValue,
    };
}
