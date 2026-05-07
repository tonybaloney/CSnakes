using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Converter;

public class BytesConverterTest(PythonEnvironmentFixture fixture) :
    ConverterTestBase<byte[], PyObjectImporters.ByteArray, BytesConverterTest>(fixture),
    IConverterTestCasesContainer<byte[]>
{
    public static TheoryData<byte[]> TestCases => new()
    {
        new byte[] { 0x01, 0x02, 0x03, 0x04 },
        new byte[] { }
    };
}
