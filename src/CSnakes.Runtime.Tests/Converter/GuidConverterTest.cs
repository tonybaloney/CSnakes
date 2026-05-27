using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Converter;

public class GuidConverterTest :
    ConverterTestBase<Guid, PyObjectImporters.Uuid, GuidConverterTest>,
    IConverterTestCasesContainer<Guid>
{
    public static TheoryData<Guid> TestCases =>
    [
        Guid.Empty,
        Guid.Parse("12345678-1234-5678-1234-567812345678"),
        Guid.Parse("550e8400-e29b-41d4-a716-446655440000")
    ];
}
