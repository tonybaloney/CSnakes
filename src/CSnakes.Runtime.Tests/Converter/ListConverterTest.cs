using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Converter;

public class ListConverterTest :
    ConverterTestBase<IReadOnlyList<long>,
                      PyObjectImporters.List<long, PyObjectImporters.Int64>,
                      ListConverterTest>,
    IConverterTestCasesContainer<IReadOnlyList<long>>
{
    public static TheoryData<IReadOnlyList<long>> TestCases => new()
    {
        new[] { 123456L, 123562 }
    };
}
