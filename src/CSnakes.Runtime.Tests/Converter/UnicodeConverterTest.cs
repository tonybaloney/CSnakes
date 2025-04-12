#pragma warning disable PRTEXP002

using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Converter;

public class UnicodeConverterTest :
    ConverterTestBase<string, PyObjectImporters.String, UnicodeConverterTest>,
    IConverterTestCasesContainer<string>
{
    public static TheoryData<string> TestCases => new()
    {
        "Hello, World!",
        "你好，世界！",
        "こんにちは、世界！",
        "안녕하세요, 세계!",
        "مرحبا بالعالم!",
        "नमस्ते दुनिया!"
    };
}
