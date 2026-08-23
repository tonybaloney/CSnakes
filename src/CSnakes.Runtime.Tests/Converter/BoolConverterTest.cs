namespace CSnakes.Runtime.Tests.Converter;

public class BoolConverterTest(PythonEnvironmentFixture fixture) :
    ConverterTestBase<bool, BoolConverterTest>(fixture),
    IConverterTestCasesContainer<bool>
{
    public static TheoryData<bool> TestCases => new() { true, false };
}
