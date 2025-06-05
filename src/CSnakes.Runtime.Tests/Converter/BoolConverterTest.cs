namespace CSnakes.Runtime.Tests.Converter;

public class BoolConverterTest :
    ConverterTestBase<bool, BoolConverterTest>,
    IConverterTestCasesContainer<bool>
{
    public static TheoryData<bool> TestCases => new() { true, false };
}
