using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Converter;

public class DateConverterTest :
    ConverterTestBase<DateOnly, PyObjectImporters.Date, DateConverterTest>,
    IConverterTestCasesContainer<DateOnly>
{
    public static TheoryData<DateOnly> TestCases =>
    [
        DateOnly.MinValue,
        DateOnly.MaxValue,
        new DateOnly(1970, 1, 1),
        new DateOnly(2026, 3, 8)
    ];
}
