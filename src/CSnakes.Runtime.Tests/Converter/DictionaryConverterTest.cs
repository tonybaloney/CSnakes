namespace CSnakes.Runtime.Tests.Converter;

public class DictionaryConverterTest : ConverterTestBase
{
    [Fact]
    public void DictionaryConverter() =>
        RunTest<IReadOnlyDictionary<string, string>>(new Dictionary<string, string>()
        {
            ["Hello"] = "World?",
            ["Foo"] = "Bar"
        });
}
