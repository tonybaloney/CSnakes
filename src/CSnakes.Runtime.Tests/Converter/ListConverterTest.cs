namespace CSnakes.Runtime.Tests.Converter;

public class ListConverterTest : ConverterTestBase
{
    [Fact]
    public void ListConverter() =>
        RunTest<IReadOnlyList<long>>([123456, 123562]);
}
