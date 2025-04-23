using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Converter;

public class DictionaryConverterTest :
    ConverterTestBase<IReadOnlyDictionary<string, string>,
                      PyObjectImporters.Dictionary<string, string, PyObjectImporters.String, PyObjectImporters.String>,
                      DictionaryConverterTest>,
    IConverterTestCasesContainer<IReadOnlyDictionary<string, string>>
{
    public static TheoryData<IReadOnlyDictionary<string, string>> TestCases => new()
    {
        new Dictionary<string, string>
        {
            ["Hello"] = "World?",
            ["Foo"] = "Bar"
        }
    };
}
