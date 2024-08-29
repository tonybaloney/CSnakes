namespace CSnakes.Runtime.Tests.Converter;

public class ListConverterTest : ConverterTestBase
{
    [Fact]
    public void IEnumerableConverter() =>
        RunTest<IReadOnlyList<string>>(["Hell0", "W0rld"]);

    [Fact]
    public void ListConverter() =>
        RunTest<List<long>>([123456, 123562]);
}
